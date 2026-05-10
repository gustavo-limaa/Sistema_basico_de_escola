using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Domain.Interfaces
{
    public interface IRepositorioMatricula
    {
        Task AdicionarAsync(Matricula matricula);

        Task AtualizarAsync(Matricula matricula);

        Task<IEnumerable<Matricula>> ListarTodasAsync();

        Task<Matricula?> ObterPorIdAsync(Guid id);

        Task<bool> ExisteMatriculaAtivaAsync(Guid estudanteId, Guid turmaId);

        Task<int> ContarMatriculasAtivasNaTurmaAsync(Guid turmaId);

        Task<bool> ExisteQualquerMatriculaAtivaParaTurmaAsync(Guid turmaId);

        Task<bool> SalvarAlteracoesAsync();
    }
}