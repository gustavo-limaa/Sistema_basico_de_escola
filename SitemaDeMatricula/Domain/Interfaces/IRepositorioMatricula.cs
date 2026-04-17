using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Interfaces
{
    public interface IRepositorioMatricula
    {
        Task AdicionarAsync(Matricula matricula);

        Task AtualizarAsync(Matricula matricula);

        Task<IEnumerable<Matricula>> ListarTodasAsync();

        Task<Matricula?> ObterPorIdAsync(Guid id);

        Task<bool> ExisteMatriculaAtivaAsync(Guid estudanteId, Guid turmaId);

        Task<bool> salvarAlteracoesAsync();
    }
}