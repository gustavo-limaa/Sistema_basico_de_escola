using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Interfaces
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

        Task<bool> SalvarAlteracoesAsync();
    }
}