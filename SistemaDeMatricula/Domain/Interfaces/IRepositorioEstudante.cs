using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Domain.Interfaces;

public interface IRepositorioEstudante
{
    Task<bool> ExisteEmailAsync(string email, Guid id);

    Task<bool> ExisteMatriculaAsync(Guid estudanteId);

    Task<bool> ExisteCpfAsync(string cpf);

    Task<Estudante?> ObterPorCpfAsync(string cpf);

    Task<IEnumerable<Estudante>> ObterTodosAsync();

    Task<Estudante?> ObterPorIdAsync(Guid estudanteId);

    Task AdicionarAsync(Estudante estudante);

    void Atualizar(Estudante estudante);

    void Remover(Estudante estudante);

    Task<bool> SalvarAlteracoesAsync();
}