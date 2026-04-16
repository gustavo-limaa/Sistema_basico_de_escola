namespace SitemaDeMatricula.Domain.Interfaces;

using SitemaDeMatricula.Domain.Modelos;

public interface IDisciplinaRepositorio
{
    Task<Disciplina?> ObterPorIdAsync(Guid id);

    Task<IEnumerable<Disciplina>> ObterTodasAsync();

    Task AdicionarAsync(Disciplina disciplina);

    void Atualizar(Disciplina disciplina);

    void Remover(Disciplina disciplina);

    Task<bool> SalvarAlteracoesAsync();

    Task<bool> AtivarDesativarAsync(Guid id, bool ativo);

    // Um bônus para o seu UseCase:
    Task<bool> ExisteDisciplinaComMesmoNomeAsync(string nome);
}