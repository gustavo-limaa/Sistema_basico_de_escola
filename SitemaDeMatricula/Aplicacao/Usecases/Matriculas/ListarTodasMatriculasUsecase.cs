using SitemaDeMatricula.Aplicacao.Dtos.Matricola;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Matriculas;

public class ListarTodasMatriculasUsecase
{
    private readonly IRepositorioMatricula _matriculaRepo;

    public ListarTodasMatriculasUsecase(IRepositorioMatricula matriculaRepo)
    {
        _matriculaRepo = matriculaRepo;
    }

    public async Task<Result<IEnumerable<MatriculaDtoResponse>>> ExecutarAsync()
    {
        var matriculas = await _matriculaRepo.ListarTodasAsync();

        // Se vier vazio, o seu Mapper de lista já deve tratar e devolver []
        var response = matriculas.ToMatriculaDtoResponseList();

        return Result<IEnumerable<MatriculaDtoResponse>>.Ok(response);
    }
}