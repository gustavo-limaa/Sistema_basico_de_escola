using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.InfraEstrutura.Data;

namespace SitemaDeMatricula.Infraestrutura.Repositorios;

public class RepositorioEstudante : IRepositorioEstudante
{
    private readonly AppDbContext _context;

    public RepositorioEstudante(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Estudante>> ObterTodosAsync()
    {
        // Repositório apenas busca. Se não tiver nada, retorna lista vazia.
        return await _context.Estudantes
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estudante?> ObterPorIdAsync(Guid estudanteId)
    {
        // Retorna o objeto ou null. O Use Case que vai dizer se isso é um erro 404.
        return await _context.Estudantes
            .FirstOrDefaultAsync(e => e.EstudanteId == estudanteId);
    }

    public async Task AdicionarAsync(Estudante estudante)
    {
        await _context.Estudantes.AddAsync(estudante);
    }

    public void Atualizar(Estudante estudante)
    {
        // No EF, se a entidade já veio do banco, ele já rastreia.
        // O Update apenas garante que o estado mude para Modified.
        _context.Estudantes.Update(estudante);
    }

    public void Remover(Estudante estudante)
    {
        _context.Estudantes.Remove(estudante);
    }

    public async Task<bool> SalvarAlteracoesAsync()
    {
        // Retorna true se salvou pelo menos uma linha
        return await _context.SaveChangesAsync() > 0;
    }
}