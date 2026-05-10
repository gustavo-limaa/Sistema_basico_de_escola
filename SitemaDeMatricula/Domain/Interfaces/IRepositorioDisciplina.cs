namespace SistemaDeMatricula.Domain.Interfaces;

using SistemaDeMatricula.Domain.Modelos;

public interface IDisciplinaRepositorio
{
    Task<Disciplina?> ObterPorIdAsync(Guid id);

    Task<IEnumerable<Disciplina>> ObterTodasAsync();

    Task AdicionarAsync(Disciplina disciplina);

    void Atualizar(Disciplina disciplina);

    void Remover(Disciplina disciplina);

    Task<bool> SalvarAlteracoesAsync();

    Task<bool> AtivarDesativarAsync(Guid id, bool ativo);

    Task<Disciplina?> ObterDesativadaPorIdAsync(Guid id);

    Task<bool> ExisteDisciplinaComMesmoNomeAsync(string nome);
}