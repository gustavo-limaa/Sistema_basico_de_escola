namespace SitemaDeMatricula.Infraestrutura.Repositorios;

using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.InfraEstrutura.Data;

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
            .FirstOrDefaultAsync(d => d.DisciplinaId == id);
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

    public async Task<bool> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExisteDisciplinaComMesmoNomeAsync(string nome)
    {
        // Aqui o EF compara a string com o seu Value Object automaticamente
        return await _context.Disciplinas.AnyAsync(d => d.Nome == nome);
    }

    public async Task<bool> AtivarDesativarAsync(Guid id, bool ativo)
    {
        var disciplina = await _context.Disciplinas.FindAsync(id);

        if (disciplina == null) return false;

        // Usando os métodos da sua Entidade Rica!
        if (ativo)
            disciplina.Ativar();
        else
            disciplina.Desativar();

        return await _context.SaveChangesAsync() > 0;
    }
}