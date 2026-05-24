using Microsoft.EntityFrameworkCore;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;

namespace SistemaDeMatricula.Infraestrutura.Repositorios
{
    public class RepositorioNotas : IRepositorioNotas
    {
        private readonly AppDbContext _contexto;

        public RepositorioNotas(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public async Task AdicionarAsync(Nota nota)
        => await _contexto.notas.AddAsync(nota);

        public async Task AtualizarAsync(Nota nota)
            => _contexto.notas.Update(nota);

        public async Task<List<Nota>> ListarTodasNotas()
           => await _contexto.notas.AsNoTracking().ToListAsync();

        public async Task<Nota?> ObterPorId(Guid id)
           => await _contexto.notas.FirstOrDefaultAsync(n => n.Id == id);
    }
}