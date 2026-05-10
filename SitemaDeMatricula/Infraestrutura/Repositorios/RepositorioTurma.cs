using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.InfraEstrutura.Data;

namespace SitemaDeMatricula.Infraestrutura.Repositorios
{
    public class RepositorioTurma : IRepositorioTurma
    {
        private readonly AppDbContext _context;

        public RepositorioTurma(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Turma turma)
        {
            await _context.Turmas.AddAsync(turma);
            await SalvarAlteracoesAsync();
        }

        public async Task<bool> AlternarStatusAsync(Turma turma)
        {
            var turmaExistente = await _context.Turmas.FindAsync(turma.TurmaId);
            if (turmaExistente == null) return false;

            if (turmaExistente.Ativo)
                turmaExistente.Desativar();
            else
                turmaExistente.Ativar();

            await SalvarAlteracoesAsync();
            return true;
        }

        public async Task<IEnumerable<Turma>> ListarTodasAsync()
        {
            return await _context.Turmas
                .Include(t => t.Professor)
                .Include(t => t.Disciplina)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Turma?> ObterPorIdAsync(Guid id)
        {
            return await _context.Turmas
                .Include(t => t.Professor)
                .Include(t => t.Disciplina)
                .FirstOrDefaultAsync(t => t.TurmaId == id);
        }

        public async Task<bool> AtualizarAsync(Turma turma)
        {
            return await SalvarAlteracoesAsync();
        }

        public async Task<bool> RemoverAsync(Turma turma)
        {
            var turmaExistente = await _context.Turmas.FindAsync(turma.TurmaId);

            if (turmaExistente == null) return false;

            turmaExistente.Desativar();

            await SalvarAlteracoesAsync();

            return true;
        }

        public async Task<Turma?> ObterPorIdIgnorandoFiltrosAsync(Guid id)
        {
            return await _context.Turmas.Include(t => t.Professor)
                .Include(t => t.Disciplina)
                .IgnoreQueryFilters().AsNoTracking()
                .FirstOrDefaultAsync(t => t.TurmaId == id);
        }

        public async Task<bool> SalvarAlteracoesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Turma?> ObterPorCodigoAsync(string codigo)
        {
            var codigoVO = CodigoTurma.CriarDeString(codigo);

            return await _context.Turmas
                .FirstOrDefaultAsync(t => t.CodigoTurma == codigoVO);
        }

        public async Task<Turma?> ObterPorCodigoIgnorandoFiltrosAsync(string codigo)
        {
            var codigoVO = CodigoTurma.CriarDeString(codigo);

            return await _context.Turmas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.CodigoTurma == codigoVO);
        }

        public async Task<bool> RestaurarAsync(Guid id)
        {
            var turmaInativa = await _context.Turmas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TurmaId == id);

            if (turmaInativa == null) return false;

            await SalvarAlteracoesAsync();
            return true;
        }
    }
}