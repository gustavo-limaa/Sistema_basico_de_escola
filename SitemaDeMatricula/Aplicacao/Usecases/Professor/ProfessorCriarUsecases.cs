using SitemaDeMatricula.Aplicaçao.Dtos.Professor;
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
        // 1. Fail Fast: Dados básicos
        if (dto == null) return Result<ProfessorDtoResponse>.Falha("Dados não fornecidos.");

        // 2. Regra de Negócio: Verificar duplicidade NO BANCO antes de qualquer coisa
        var professorExistente = await _repositorioProfessor.ObterPorCpfAsync(dto.Cpf);

        if (professorExistente != null)
        {
            return Result<ProfessorDtoResponse>.Falha("Já existe um professor cadastrado com este CPF.");
        }

        try
        {
            // 3. Mapeamento (Aqui os seus Value Objects são criados e validados)
            var professor = dto.ToProfessor();

            // 4. Persistência (Só chegamos aqui se tudo estiver OK)
            await _repositorioProfessor.AdicionarAsync(professor);

            var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

            if (!sucesso)
            {
                return Result<ProfessorDtoResponse>.Falha("Erro ao persistir os dados no banco.");
            }

            // 5. Retorno limpo
            return Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse());
        }
        catch (Exception ex)
        {
            // Se o seu Value Object lançar uma exceção de validação, você captura aqui
            return Result<ProfessorDtoResponse>.Falha($"Erro de validação: {ex.Message}");
        }
    }
}