using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorObterTodosUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorObterTodosUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<IEnumerable<ProfessorDtoResponse>>> ExecutarAsync()
    {
        var professores = await _repositorioProfessor.ObterTodosAsync();

        var professoresDto = professores.Select(p => p.ToProfessorDtoResponse()).ToList();

        return Result<IEnumerable<ProfessorDtoResponse>>.Ok(professoresDto);
    }
}