using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

// Corrigido o nome para Restaurar
public class ProfessorRestaurarUseCase
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorRestaurarUseCase(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(Guid professorId)
    {
        if (professorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha("ID do professor é inválido.");

        try
        {
            var professor = await _repositorioProfessor.ObterPorIdIgnorandoFiltrosAsync(professorId);

            if (professor == null)
                return Result<ProfessorDtoResponse>.Falha("Professor não encontrado.");

            if (professor.Ativo)
                return Result<ProfessorDtoResponse>.Conflito("Este professor já está ativo e não precisa ser restaurado.");

            professor.Ativar();
            _repositorioProfessor.Atualizar(professor);

            var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

            return sucesso
                // Retornamos o DTO do professor para satisfazer o tipo Result<T>
                ? Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse())
                : Result<ProfessorDtoResponse>.Falha("Erro ao persistir os dados no banco.");
        }
        catch (Exception ex)
        {
            return Result<ProfessorDtoResponse>.Falha($"Erro inesperado: {ex.Message}");
        }
    }
}