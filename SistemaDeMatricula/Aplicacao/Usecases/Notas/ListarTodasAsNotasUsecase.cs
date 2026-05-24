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

    public async Task<Result<IEnumerable<NotaDtoResponse>>> ExecuteAsAsync(Guid matriculaId)
    {
        var matricula = await _uow.Matriculas.ObterPorIdAsync(matriculaId);
        if (matricula == null)
            return Result<IEnumerable<NotaDtoResponse>>.NaoEncontrado("Matrícula não encontrada.");

        var notasDto = matricula.Notas.Select(n => n.ToNotaDtoResponse());

        return Result<IEnumerable<NotaDtoResponse>>.Ok(notasDto);
    }
}