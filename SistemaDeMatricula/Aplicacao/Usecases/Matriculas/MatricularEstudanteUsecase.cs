using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Events;
using SistemaDeMatricula.Services;

using SistemaDeMatricula.Domain.Erros;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

public sealed class MatricularEstudanteUsecase
{
    private readonly IUnitOfWork _uow;
    private readonly IRabbitMqProducer _rabbitMqProducer;

    public MatricularEstudanteUsecase(IUnitOfWork uow, IRabbitMqProducer rabbitMqProducer)
    {
        _uow = uow;
        _rabbitMqProducer = rabbitMqProducer;
    }

    public async Task<Result<MatriculaDtoResponse>> ExecutarAsync(MatriculaDtoCreate dto)
    {
        var estudante = await _uow.Estudantes.ObterPorIdAsync(dto.EstudanteId);
        if (estudante == null) return Result<MatriculaDtoResponse>.Falha(MensagensEstudante.ErroEstudanteIdInvalido);

        var turma = await _uow.Turmas.ObterPorIdAsync(dto.TurmaId);
        if (turma == null) return Result<MatriculaDtoResponse>.Falha(MensagensTurma.TurmaNaoEncontrada);

        if (await _uow.Matriculas.ExisteMatriculaAtivaAsync(dto.EstudanteId, dto.TurmaId))
        {
            return Result<MatriculaDtoResponse>.Falha(MensagensMatricula.MatriculaJaExistente);
        }

        var totalMatriculados = await _uow.Matriculas.ContarMatriculasAtivasNaTurmaAsync(dto.TurmaId);

        if (!turma.TemVagaDisponivel(totalMatriculados))
        {
            return Result<MatriculaDtoResponse>.Falha(MensagensTurma.TurmaLotada);
        }
        var novaMatricula = new Matricula(dto.EstudanteId, dto.TurmaId);

        await _uow.Matriculas.AdicionarAsync(novaMatricula);

        var sucesso = await _uow.CommitAsync();

        if (!sucesso)
        {
            return Result<MatriculaDtoResponse>.Falha(MensagensMatricula.ErroPersistenciaBanco);
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