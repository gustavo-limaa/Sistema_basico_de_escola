using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorCriarUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorCriarUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(ProfessorDtoCreate dto)
    {
        if (dto == null) return Result<ProfessorDtoResponse>.Falha("Dados não fornecidos.");

        try
        {
            var professor = dto.ToProfessor();

            var professorExistenteCpf = await _repositorioProfessor.ObterPorCpfAsync(dto.Cpf);
            if (professorExistenteCpf != null)
                return Result<ProfessorDtoResponse>.Conflito("Já existe um professor cadastrado com este CPF.");

            var professorExistenteEmail = await _repositorioProfessor.ObterPorEmailAsync(dto.Email);
            if (professorExistenteEmail != null)
                return Result<ProfessorDtoResponse>.Conflito("Já existe um professor cadastrado com este e-mail.");
            await _repositorioProfessor.AdicionarAsync(professor);
            var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

            return sucesso
                ? Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse())
                : Result<ProfessorDtoResponse>.Falha("Erro ao persistir os dados no banco.");
        }
        catch (ArgumentException ex)
        {
            return Result<ProfessorDtoResponse>.Falha(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProfessorDtoResponse>.Falha($"Erro inesperado: {ex.Message}");
        }
    }
}