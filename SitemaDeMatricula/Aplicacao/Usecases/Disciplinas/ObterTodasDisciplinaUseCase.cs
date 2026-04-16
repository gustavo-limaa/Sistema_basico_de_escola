using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases.Disciplinas;

public class ObterTodasDisciplinaUseCase
{
    private readonly IDisciplinaRepositorio _disciplinaRepositorio;

    public ObterTodasDisciplinaUseCase(IDisciplinaRepositorio disciplinaRepositorio)
    {
        _disciplinaRepositorio = disciplinaRepositorio;
    }

    public async Task<Result<IEnumerable<DisciplinaDtoResponse>>> Executar()
    {
        var disciplinas = await _disciplinaRepositorio.ObterTodasAsync();

        // Transformamos a lista de Entidades em uma lista de DTOs de Resposta
        var dtos = disciplinas.Select(d => d.ToResponse());

        return Result<IEnumerable<DisciplinaDtoResponse>>.Ok(dtos);
    }
}