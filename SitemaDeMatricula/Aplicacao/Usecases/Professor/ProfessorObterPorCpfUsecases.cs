using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorObterPorCpfUsecases
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