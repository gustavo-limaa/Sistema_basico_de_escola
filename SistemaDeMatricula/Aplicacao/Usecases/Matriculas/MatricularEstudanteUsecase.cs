using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Events;
using SistemaDeMatricula.Services;
using Xunit;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

public sealed class MatricularEstudanteUsecase
{
    private readonly IUnitOfWork _uow;
    private readonly RabbitMqProducer _rabbitMqProducer;

    public MatricularEstudanteUsecase(IUnitOfWork uow, RabbitMqProducer rabbitMqProducer)
    {
        _uow = uow;
        _rabbitMqProducer = rabbitMqProducer;
    }

    public async Task<Result<MatriculaDtoResponse>> ExecutarAsync(MatriculaDtoCreate dto)
    {
        var estudante = await _uow.Estudantes.ObterPorIdAsync(dto.EstudanteId);
        if (estudante == null) return Result<MatriculaDtoResponse>.Falha("Estudante não encontrado.");

        var turma = await _uow.Turmas.ObterPorIdAsync(dto.TurmaId);
        if (turma == null) return Result<MatriculaDtoResponse>.Falha("Turma não encontrada.");

        if (await _uow.Matriculas.ExisteMatriculaAtivaAsync(dto.EstudanteId, dto.TurmaId))
        {
            return Result<MatriculaDtoResponse>.Falha("Este estudante já está matriculado nesta turma.");
        }

        var totalMatriculados = await _uow.Matriculas.ContarMatriculasAtivasNaTurmaAsync(dto.TurmaId);

        if (!turma.TemVagaDisponivel(totalMatriculados))
        {
            return Result<MatriculaDtoResponse>.Falha("Turma lotada! Capacidade máxima atingida.");
        }
        var novaMatricula = new Matricula(dto.EstudanteId, dto.TurmaId);

        await _uow.Matriculas.AdicionarAsync(novaMatricula);

        var sucesso = await _uow.CommitAsync();

        if (!sucesso)
        {
            return Result<MatriculaDtoResponse>.Falha("Ocorreu um erro ao persistir a matrícula no banco de dados.");
        }

        var evento = new MatriculaSolicitadaEvent
        {
            AlunoId = dto.EstudanteId,
            TurmaId = dto.TurmaId,
            UsuarioId = "SistemaDeMatricula.API",
            Origem = "SistemaDeMatricula.API"
        };

        await _rabbitMqProducer.EnviarMensagemAsync(evento, "escola_matriculas_exchange");

        return Result<MatriculaDtoResponse>.Ok(novaMatricula.ToMatriculaDtoResponse());
    }
}