using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

public sealed class ObterMatriculaPorIdUsecase
{
    private readonly IUnitOfWork _uow;

    public ObterMatriculaPorIdUsecase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<MatriculaDtoResponse>> ExecutarAsync(Guid id)
    {
        if (id == Guid.Empty)
            return Result<MatriculaDtoResponse>.Falha(MensagensMatricula.MatriculaNaoEncontrada);

        var matricula = await _uow.Matriculas.ObterPorIdAsync(id);

        if (matricula is null)
            return Result<MatriculaDtoResponse>.NaoEncontrado(MensagensMatricula.MatriculaNaoEncontrada);

        return Result<MatriculaDtoResponse>.Ok(matricula.ToMatriculaDtoResponse());
    }
}