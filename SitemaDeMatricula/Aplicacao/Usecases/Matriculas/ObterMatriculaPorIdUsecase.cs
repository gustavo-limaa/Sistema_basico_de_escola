using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

public sealed class ObterMatriculaPorIdUsecase
{
    private readonly IRepositorioMatricula _matriculaRepo;

    public ObterMatriculaPorIdUsecase(IRepositorioMatricula matriculaRepo)
    {
        _matriculaRepo = matriculaRepo;
    }

    public async Task<Result<MatriculaDtoResponse>> ExecutarAsync(Guid id)
    {
        var matricula = await _matriculaRepo.ObterPorIdAsync(id);

        if (matricula == null)
            return Result<MatriculaDtoResponse>.Falha("Matrícula não encontrada.");

        return Result<MatriculaDtoResponse>.Ok(matricula.ToMatriculaDtoResponse());
    }
}