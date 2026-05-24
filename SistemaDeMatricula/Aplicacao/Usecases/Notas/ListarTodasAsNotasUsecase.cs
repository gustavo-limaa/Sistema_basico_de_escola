using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas;

public class ListarTodasAsNotasUsecase
{
    private readonly IUnitOfWork _uow;

    public ListarTodasAsNotasUsecase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IEnumerable<NotaDtoResponse>>> ExecuteAsAsync()
    {
        var notas = await _uow.Notas.ListarTodasNotas
            ();

        return Result<IEnumerable<NotaDtoResponse>>.Ok(notas.Select(n => n.ToNotaDtoResponse()));
    }
}