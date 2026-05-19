using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

public sealed class ProfessorObterPorCpfUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorObterPorCpfUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return Result<ProfessorDtoResponse>.Falha("CPF é obrigatório.");
        var professor = await _repositorioProfessor.ObterPorCpfAsync(cpf);
        if (professor == null)
            return Result<ProfessorDtoResponse>.Falha("Professor não encontrado.");
        return Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse());
    }
}