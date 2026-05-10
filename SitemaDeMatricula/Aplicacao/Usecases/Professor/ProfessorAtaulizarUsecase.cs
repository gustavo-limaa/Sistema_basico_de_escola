using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

public sealed class ProfessorAtualizarUsecase
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorAtualizarUsecase(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(ProfessorDtoUpdate professorDto)
    {
        if (professorDto == null || professorDto.ProfessorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha("Dados do professor são inválidos.");
        var professorExistente = await _repositorioProfessor.ObterPorIdAsync(professorDto.ProfessorId);
        if (professorExistente == null)
            return Result<ProfessorDtoResponse>.Falha("Professor não encontrado.");

        var professorComMesmoEmail = await _repositorioProfessor.ObterPorEmailAsync(professorDto.Email);

        if (professorComMesmoEmail != null && professorComMesmoEmail.Id != professorDto.ProfessorId)
        {
            return Result<ProfessorDtoResponse>.Falha("Já existe outro professor cadastrado com este e-mail.");
        }
        professorExistente.ToAtualizarProfessor(professorDto);

        _repositorioProfessor.Atualizar(professorExistente);
        var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();
        return sucesso ? Result<ProfessorDtoResponse>.Ok(professorExistente.ToProfessorDtoResponse()) : Result<ProfessorDtoResponse>.Falha("Erro ao atualizar o professor.");
    }
}