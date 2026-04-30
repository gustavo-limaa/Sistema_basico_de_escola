using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

public class RemoverTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;

    public RemoverTurmaUseCase(IRepositorioTurma turmaRepo)
    {
        _turmaRepo = turmaRepo;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var turma = await _turmaRepo.ObterPorIdAsync(id);

        if (turma == null)
            return Result<bool>.Falha("Turma não encontrada.");

        // Se já estiver inativa, apenas confirmamos o sucesso (Idempotência)
        if (!turma.Ativo)
            return Result<bool>.Ok(true);

        // Usamos o método específico de remoção/soft delete que você refinou
        var sucesso = await _turmaRepo.RemoverAsync(turma);

        return sucesso
            ? Result<bool>.Ok(true)
            : Result<bool>.Falha("Erro ao desativar a turma.");
    }
}