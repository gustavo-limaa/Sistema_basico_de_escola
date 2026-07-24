using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
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
            return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorInvalido);
        var professor = await _repositorioProfessor.ObterPorCpfAsync(cpf);
        if (professor == null)
            return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorNaoEncontrado);
        return Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse());
    }
}