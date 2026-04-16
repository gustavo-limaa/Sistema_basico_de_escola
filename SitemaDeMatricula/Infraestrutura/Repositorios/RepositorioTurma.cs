using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
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

            turmaExistente.AlternarStatus();

            await SalvarAlteracoesAsync();
            return true;
        }

        public async Task<IEnumerable<Turma>> ListarTodasAsync()
        {
            return await _context.Turmas
                .Include(t => t.Professor)
                .Include(t => t.Disciplina)
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
            var turmaExistente = await _context.Turmas.FindAsync(turma.TurmaId);

            if (turmaExistente == null)
                return false;

            // Atualiza as propriedades da turma existente com os valores da turma fornecida
            _context.Entry(turmaExistente).CurrentValues.SetValues(turma);
            await SalvarAlteracoesAsync();
            return true;
        }

        public async Task<Turma?> ObterPorCodigoAsync(string codigo)
        {
            return await _context.Turmas.FirstOrDefaultAsync(t => t.CodigoTurma == codigo);
        }

        public async Task<bool> RemoverAsync(Turma turma)
        {
            var turmaExistente = await _context.Turmas.FindAsync(turma.TurmaId);
            if (turmaExistente == null) return false;
            _context.Turmas.Remove(turmaExistente);
            await SalvarAlteracoesAsync();
            return true;
        }

        public async Task<bool> SalvarAlteracoesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}