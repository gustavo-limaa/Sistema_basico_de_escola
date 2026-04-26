using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Interfaces;

public interface IRepositorioEstudante
{
    Task<bool> ExisteEmailAsync(string email, Guid id);

    Task<bool> ExisteMatriculaAsync(Guid estudanteId);

    Task<bool> ExisteCpfAsync(string cpf);

    Task<IEnumerable<Estudante>> ObterTodosAsync();

    Task<Estudante?> ObterPorIdAsync(Guid estudanteId); // Pode retornar null se não achar

    Task AdicionarAsync(Estudante estudante);

    void Atualizar(Estudante estudante); // Geralmente void porque o EF já rastreia

    void Remover(Estudante estudante);

    Task<bool> SalvarAlteracoesAsync(); // O método que realmente "comita" no banco
}