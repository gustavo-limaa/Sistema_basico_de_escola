using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.InfraEstrutura.Data;

namespace SitemaDeMatricula.Infraestrutura.Repositorios
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
            await salvarAlteracoesAsync();
        }

        public async Task AtualizarAsync(Matricula matricula)
        {
            _appDbContext.Matriculas.Update(matricula);
            await salvarAlteracoesAsync();
        }

        public async Task<bool> ExisteMatriculaAtivaAsync(Guid estudanteId, Guid turmaId)
        {
            if (estudanteId == Guid.Empty || turmaId == Guid.Empty)
                return false;

            return await _appDbContext.Matriculas.AnyAsync(m => m.EstudanteId == estudanteId && m.TurmaId == turmaId && m.Ativo);
        }

        public async Task<IEnumerable<Matricula>> ListarTodasAsync()
        {
            return await _appDbContext.Matriculas.AsNoTracking().Include(m => m.Estudante).Include(m => m.Turma).ToListAsync();
        }

        public async Task<Matricula?> ObterPorIdAsync(Guid id)
        {
            return await _appDbContext.Matriculas.AsNoTracking().Include(m => m.Estudante).Include(m => m.Turma).FirstOrDefaultAsync(m => m.MatriculaId == id);
        }

        public async Task<bool> salvarAlteracoesAsync()
        {
            return await _appDbContext.SaveChangesAsync() > 0;
        }
    }
}