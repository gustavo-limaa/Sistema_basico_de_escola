using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turma;

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

        // Em vez de apagar do banco, apenas mudamos o estado
        // Se ela já estiver desativada, não fazemos nada ou avisamos
        if (!turma.Ativo)
            return Result<bool>.Ok(true);

        // Chama o método que criamos no repositório que faz o toggle ou desativa
        var sucesso = await _turmaRepo.AlternarStatusAsync(turma);

        return sucesso
            ? Result<bool>.Ok(true)
            : Result<bool>.Falha("Erro ao desativar a turma.");
    }
}