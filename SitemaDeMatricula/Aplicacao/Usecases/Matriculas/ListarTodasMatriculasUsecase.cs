using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

public sealed class ListarTodasMatriculasUsecase
{
    private readonly IUnitOfWork _uow;

    public ListarTodasMatriculasUsecase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IEnumerable<MatriculaDtoResponse>>> ExecutarAsync()
    {
        try
        {
            var matriculas = await _uow.Matriculas.ListarTodasAsync();

            if (matriculas == null)
                return Result<IEnumerable<MatriculaDtoResponse>>.Ok(Enumerable.Empty<MatriculaDtoResponse>());

            var response = matriculas.ToMatriculaDtoResponseList();

            return Result<IEnumerable<MatriculaDtoResponse>>.Ok(response);
        }
        catch (Exception ex)
        {
            // Agora sim o Use Case trata o erro e devolve uma falha amigável!
            return Result<IEnumerable<MatriculaDtoResponse>>.Falha($"Ocorreu um erro ao listar as matrículas: {ex.Message}");
        }
    }
}