using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Interfaces;

public interface IRepositorioEstudante
{
    Task<Result<IEnumerable<Estudante>>> ObterTodosAsync();

    Task<Result<Estudante>> ObterPorIdAsync(Guid estudanteId);

    Task<Result<Estudante>> AdicionarAsync(Estudante estudante);

    Task<Result<Estudante>> AtualizarAsync(Estudante estudante);

    Task<Result<bool>> RemoverAsync(Guid estudanteId);
}