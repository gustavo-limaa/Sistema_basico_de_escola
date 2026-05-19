using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Domain.Interfaces
{
    public interface IRepositorioProfessor
    {
        Task<Professor?> ObterPorIdAsync(Guid professorId);

        Task<Professor?> ObterPorCpfAsync(string cpf);

        Task<IEnumerable<Professor>> ObterTodosAsync();

        Task AdicionarAsync(Professor professor);

        public void Atualizar(Professor professor);

        Task<Professor?> ObterPorIdIgnorandoFiltrosAsync(Guid id);

        Task<bool> SalvarAlteracoesAsync();

        Task<Professor?> ObterPorEmailAsync(string email);

        Task<bool> ExisteTurmaAtivaParaProfessorAsync(Guid professorId);
    }
}