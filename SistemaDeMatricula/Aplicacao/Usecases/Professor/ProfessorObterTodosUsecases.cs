using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

public sealed class ProfessorObterTodosUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorObterTodosUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<List<ProfessorDtoResponse>>> ExecutarAsync()
    {
        var professores = await _repositorioProfessor.ObterTodosAsync();

        var professoresDto = professores.Select(p => p.ToProfessorDtoResponse()).ToList();

        return Result<List<ProfessorDtoResponse>>.Ok(professoresDto);
    }
}