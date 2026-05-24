using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Domain.Interfaces
{
    public interface IRepositorioNotas
    {
        Task AdicionarAsync(Nota nota);

        Task AtualizarAsync(Nota nota);

        Task<List<Nota>> ListarTodasNotas();

        Task<Nota> ObterPorId(Guid id);
    }
}