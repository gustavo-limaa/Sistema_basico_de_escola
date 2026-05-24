using Microsoft.EntityFrameworkCore;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;

namespace SistemaDeMatricula.Infraestrutura.Repositorios
{
    public class RepositorioMatricula : IRepositorioMatricula
    {
        private readonly AppDbContext _appDbContext;

        public RepositorioMatricula(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AdicionarAsync(Matricula matricula)
        {
            await _appDbContext.Matriculas.AddAsync(matricula);
        }

        public async Task AtualizarAsync(Matricula matricula)
        {
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<int> ContarMatriculasAtivasNaTurmaAsync(Guid turmaId)
        {
            return await _appDbContext.Matriculas.IgnoreQueryFilters()
        .CountAsync(m => m.TurmaId == turmaId && m.Ativo);
        }

        public async Task<bool> ExisteMatriculaAtivaAsync(Guid estudanteId, Guid turmaId)
        {
            if (estudanteId == Guid.Empty || turmaId == Guid.Empty)
                return false;

            return await _appDbContext.Matriculas.AnyAsync(m => m.EstudanteId == estudanteId && m.TurmaId == turmaId && m.Ativo);
        }

        public async Task<bool> ExisteQualquerMatriculaAtivaParaTurmaAsync(Guid turmaId)
        {
            return await _appDbContext.Matriculas
        .AnyAsync(m => m.TurmaId == turmaId && m.Ativo);
        }

        public async Task<IEnumerable<Matricula>> ListarTodasAsync()
        {
            return await _appDbContext.Matriculas.AsNoTracking().Include(m => m.Notas).Include(m => m.Estudante).Include(m => m.Turma).ToListAsync();
        }

        public async Task<Matricula?> ObterPorIdAsync(Guid id)
        {
            return await _appDbContext.Matriculas.Include(m => m.Notas).Include(m => m.Estudante).Include(m => m.Turma).FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}