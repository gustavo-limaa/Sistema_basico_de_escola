using SitemaDeMatricula.Aplicacao.Dtos.Matricola;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Matriculas;

public class ObterMatriculaPorIdUsecase
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