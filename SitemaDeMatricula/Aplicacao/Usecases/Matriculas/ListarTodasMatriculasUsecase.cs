using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

public sealed class ListarTodasMatriculasUsecase
{
    private readonly IRepositorioMatricula _matriculaRepo;

    public ListarTodasMatriculasUsecase(IRepositorioMatricula matriculaRepo)
    {
        _matriculaRepo = matriculaRepo;
    }

    public async Task<Result<IEnumerable<MatriculaDtoResponse>>> ExecutarAsync()
    {
        var matriculas = await _matriculaRepo.ListarTodasAsync();

        var response = matriculas.ToMatriculaDtoResponseList();

        return Result<IEnumerable<MatriculaDtoResponse>>.Ok(response);
    }
}