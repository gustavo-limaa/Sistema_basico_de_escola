namespace SistemaDeMatricula.Infraestrutura.Repositorios;

using Microsoft.EntityFrameworkCore;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;

public class DisciplinaRepositorio : IDisciplinaRepositorio
{
    private readonly AppDbContext _context;

    public DisciplinaRepositorio(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Disciplina?> ObterPorIdAsync(Guid id)
    {
        return await _context.Disciplinas.Include(d => d.Turmas)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Disciplina>> ObterTodasAsync()
    {
        return await _context.Disciplinas
            .Where(d => d.Ativo).Include(d => d.Turmas).ToListAsync();
    }

    public async Task AdicionarAsync(Disciplina disciplina)
    {
        await _context.Disciplinas.AddAsync(disciplina);
    }

    public void Atualizar(Disciplina disciplina)
    {
        _context.Disciplinas.Update(disciplina);
    }

    public void Remover(Disciplina disciplina)
    {
        _context.Disciplinas.Remove(disciplina);
    }

    public async Task<Disciplina?> ObterDesativadaPorIdAsync(Guid id)
    {
        return await _context.Disciplinas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<bool> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExisteDisciplinaComMesmoNomeAsync(string nome)
    {
        return await _context.Disciplinas.AnyAsync(d => d.Nome == nome);
    }

    public async Task<bool> AtivarDesativarAsync(Guid id, bool ativo)
    {
        var disciplina = await _context.Disciplinas.FindAsync(id);

        if (disciplina == null) return false;

        if (ativo)
            disciplina.Ativar();
        else
            disciplina.Desativar();

        return await _context.SaveChangesAsync() > 0;
    }
}