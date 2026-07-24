using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

public sealed class ProfessorObterPorIdUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorObterPorIdUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(Guid professorId)
    {
        if (professorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorInvalido);

        var professor = await _repositorioProfessor.ObterPorIdAsync(professorId);

        if (professor == null)
            return Result<ProfessorDtoResponse>.NaoEncontrado(MensagensProfessor.ProfessorNaoEncontrado);
        return Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse());
    }
}