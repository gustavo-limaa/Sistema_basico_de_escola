using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorObterPorIdUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorObterPorIdUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(Guid professorId)
    {
        if (professorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha("ID do professor é inválido.");

        var professor = await _repositorioProfessor.ObterPorIdAsync(professorId);

        if (professor == null)
            return Result<ProfessorDtoResponse>.NaoEncontrado("Professor não encontrado.");
        return Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse());
    }
}