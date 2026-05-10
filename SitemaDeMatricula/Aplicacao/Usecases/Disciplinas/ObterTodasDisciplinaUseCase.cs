using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Disciplinas;

public sealed class ObterTodasDisciplinaUseCase
{
    private readonly IDisciplinaRepositorio _disciplinaRepositorio;

    public ObterTodasDisciplinaUseCase(IDisciplinaRepositorio disciplinaRepositorio)
    {
        _disciplinaRepositorio = disciplinaRepositorio;
    }

    public async Task<Result<IEnumerable<DisciplinaDtoResponse>>> Executar()
    {
        var disciplinas = await _disciplinaRepositorio.ObterTodasAsync();

        var dtos = disciplinas.Select(d => d.ToResponse());

        return Result<IEnumerable<DisciplinaDtoResponse>>.Ok(dtos);
    }
}