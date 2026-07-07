using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas;

public sealed class RestaurarTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;

    public RestaurarTurmaUseCase(IRepositorioTurma turmaRepo)
    {
        _turmaRepo = turmaRepo;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var turma = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(id);

        if (turma == null)
            return Result<bool>.Falha("Turma não encontrada ou já está ativa.");

        // Chama o método específico de restauração que ativa a turma e salva as alterações
        var sucesso = await _turmaRepo.RestaurarAsync(id);
        return sucesso ? Result<bool>.Ok(true) : Result<bool>.Falha("Erro ao tentar restaurar a turma.");
    }
}// mapeamento de erros para código HTTP
