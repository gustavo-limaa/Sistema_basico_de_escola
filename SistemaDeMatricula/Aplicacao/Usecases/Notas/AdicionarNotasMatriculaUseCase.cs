using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas;

public sealed class AdicionarNotasMatriculaUseCase
{
    private readonly IUnitOfWork _uow;

    public AdicionarNotasMatriculaUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> ExecutarAsync(Guid matriculaId, NotaDtoCreate novaNota)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, NotaDtoCreate notaDtoCreate)
    {
        var matricula = await _uow.Matriculas.ObterPorIdAsync(matriculaId);
        if (matricula == null)
            return Result<NotaDtoResponse>.NaoEncontrado(MensagensMatricula.MatriculaNaoEncontrada);

        if (!matricula.Ativo)
            return Result<NotaDtoResponse>.Falha(MensagensMatricula.MatriculaJaDesativada);

        var nota = notaDtoCreate.ToNota(matriculaId);

        await _uow.Notas.AdicionarAsync(nota);

        // Commit via UnitOfWork
        var sucesso = await _uow.CommitAsync();
        if (!sucesso)
            return Result<NotaDtoResponse>.Falha(MensagensNotas.ErroBancoDeDados);

        return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
    }
}