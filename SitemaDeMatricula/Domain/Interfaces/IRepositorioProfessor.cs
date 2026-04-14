using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Interfaces
{
    public interface IRepositorioProfessor
    {
        // Retorna o objeto ou null. O Use Case decide se esse null é uma falha.
        Task<Professor?> ObterPorIdAsync(Guid professorId);

        Task<Professor?> ObterPorCpfAsync(string cpf);

        Task<IEnumerable<Professor>> ObterTodosAsync();

        // Operações de escrita geralmente não precisam retornar o objeto no Result aqui
        Task AdicionarAsync(Professor professor);

        public void Atualizar(Professor professor);

        public void Remover(Professor professor);

        // O "SaveAsync" pode estar aqui ou em uma Unit of Work
        Task<bool> SalvarAlteracoesAsync();
    }
}