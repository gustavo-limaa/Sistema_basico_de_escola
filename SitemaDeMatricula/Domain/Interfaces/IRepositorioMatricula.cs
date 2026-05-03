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

        // Essencial para a regra de Capacidade Máxima
        Task<int> ContarMatriculasAtivasNaTurmaAsync(Guid turmaId);

        // Essencial para a regra de "Não fechar turma com alunos"
        Task<bool> ExisteQualquerMatriculaAtivaParaTurmaAsync(Guid turmaId);

        Task<bool> SalvarAlteracoesAsync();
    }
}