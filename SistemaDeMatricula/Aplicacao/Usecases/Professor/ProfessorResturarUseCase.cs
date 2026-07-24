using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

// Corrigido o nome para Restaurar
public sealed class ProfessorRestaurarUseCase
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorRestaurarUseCase(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(Guid professorId)
    {
        if (professorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorInvalido);

        try
        {
            var professor = await _repositorioProfessor.ObterPorIdIgnorandoFiltrosAsync(professorId);

            if (professor == null)
                return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorNaoEncontrado);

            if (professor.Ativo)
                return Result<ProfessorDtoResponse>.Conflito(MensagensProfessor.ErroInativo_ou_Ativo);

            professor.Ativar();
            _repositorioProfessor.Atualizar(professor);

            var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

            return sucesso
                ? Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse())
                : Result<ProfessorDtoResponse>.Falha(MensagensProfessor.FalhaAoPersistirDados);
        }
        catch (Exception ex)
        {
            return Result<ProfessorDtoResponse>.Falha($"Erro inesperado: {ex.Message}");
        }
    }
}