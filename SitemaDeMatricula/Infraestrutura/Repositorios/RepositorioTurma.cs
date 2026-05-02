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
            // Buscamos a turma para garantir que ela está sendo rastreada pelo Contexto
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

        public async Task<Turma?> ObterPorCodigoAsync(string codigo)
        {
            return await _context.Turmas.FirstOrDefaultAsync(t => t.CodigoTurma.ValorFormatado == codigo);
        }

        public async Task<bool> RemoverAsync(Turma turma)
        {
            // 1. Buscamos para garantir que o EF está rastreando a instância real do banco
            var turmaExistente = await _context.Turmas.FindAsync(turma.TurmaId);

            if (turmaExistente == null) return false;

            // 2. Mudamos apenas o estado que nos interessa
            turmaExistente.Desativar();

            // 3. Persistimos a mudança. O EF sabe exatamente o que fazer aqui.
            await SalvarAlteracoesAsync();

            return true;
        }

        public async Task<Turma?> ObterPorIdIgnorandoFiltrosAsync(Guid id)
        {
            return await _context.Turmas.Include(t => t.Professor)
                .Include(t => t.Disciplina)
                .IgnoreQueryFilters().AsNoTracking()  // 👈 A chave para ver os "fantasmas" (inativos)
                .FirstOrDefaultAsync(t => t.TurmaId == id);
        }

        public async Task<bool> SalvarAlteracoesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Turma?> ObterPorCodigoAsync(CodigoTurma codigo)
        {
            return await _context.Turmas.FirstOrDefaultAsync(t => t.CodigoTurma == codigo);
        }

        public Task<Turma?> ObterPorCodigoIgnorandoFiltrosAsync(string codigo)
        {
            return _context.Turmas.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.CodigoTurma.ValorFormatado == codigo);
        }
    }
}