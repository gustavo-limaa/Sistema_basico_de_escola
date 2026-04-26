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
        // 1. Fail Fast Simples
        if (dto == null) return Result<ProfessorDtoResponse>.Falha("Dados não fornecidos.");

        try
        {
            // 2. O Domínio valida a lógica de formato (CPF, Email, Salário)
            // Se o DTO tiver lixo, o ToProfessor() estoura uma exceção de validação aqui mesmo
            var professor = dto.ToProfessor();

            // 3. O Use Case foca na Regra de Negócio que exige o Banco
            var professorExistente = await _repositorioProfessor.ObterPorCpfAsync(dto.Cpf);
            if (professorExistente != null)
                return Result<ProfessorDtoResponse>.Falha("Já existe um professor cadastrado com este CPF.");

            // 4. Persistência
            await _repositorioProfessor.AdicionarAsync(professor);
            var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

            return sucesso
                ? Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse())
                : Result<ProfessorDtoResponse>.Falha("Erro ao persistir os dados no banco.");
        }
        catch (ArgumentException ex) // Ou a sua Exception personalizada de Domínio
        {
            return Result<ProfessorDtoResponse>.Falha(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProfessorDtoResponse>.Falha($"Erro inesperado: {ex.Message}");
        }
    }
}