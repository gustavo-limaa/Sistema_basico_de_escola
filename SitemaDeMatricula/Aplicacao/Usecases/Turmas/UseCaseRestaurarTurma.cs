using global::SitemaDeMatricula.Domain;
using global::SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

public class RestaurarTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;

    public RestaurarTurmaUseCase(IRepositorioTurma turmaRepo)
    {
        _turmaRepo = turmaRepo;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        // 1. O Repositório deve usar .IgnoreQueryFilters() internamente para achar a turma inativa
        var turma = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(id);

        if (turma == null)
            return Result<bool>.Falha("Turma não encontrada ou já está ativa.");

        // 2. Idempotência: Se por algum motivo ela já estiver ativa, apenas retornamos sucesso
        if (turma.Ativo)
            return Result<bool>.Ok(true);

        // 3. Aciona o comportamento de domínio
        turma.Ativar();

        // 4. Persiste a mudança via Repositório
        var sucesso = await _turmaRepo.AtualizarAsync(turma);

        return sucesso
            ? Result<bool>.Ok(true)
            : Result<bool>.Falha("Erro ao tentar restaurar a turma.");
    }
}