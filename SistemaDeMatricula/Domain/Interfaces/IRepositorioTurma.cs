using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Domain.Interfaces
{
    public interface IRepositorioTurma
    {
        Task<Turma?> ObterPorIdAsync(Guid id);

        Task<IEnumerable<Turma>> ListarTodasAsync();

        Task AdicionarAsync(Turma turma);

        Task<bool> AtualizarAsync(Turma turma);

        Task<bool> RemoverAsync(Turma turma);

        Task<Turma?> ObterPorCodigoAsync(string codigo);

        Task<bool> AlternarStatusAsync(Turma turma);

        Task<Turma?> ObterPorIdIgnorandoFiltrosAsync(Guid id);

        Task<Turma?> ObterPorCodigoIgnorandoFiltrosAsync(string codigo);

        Task<bool> SalvarAlteracoesAsync();

        Task<bool> RestaurarAsync(Guid id);
    }
}