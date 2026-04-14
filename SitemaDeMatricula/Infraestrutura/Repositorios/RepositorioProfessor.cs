using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.InfraEstrutura.Data;

namespace SitemaDeMatricula.Infraestrutura.Repositorios;

public class RepositorioProfessor : IRepositorioProfessor
{
    private readonly AppDbContext _context;

    public RepositorioProfessor(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Professor professor)
    {
        await _context.Professores.AddAsync(professor);
    }

    public void Atualizar(Professor professor)
    {
        _context.Professores.Update(professor);
    }

    public async Task<Professor?> ObterPorCpfAsync(string cpf)
    {
        return await _context.Professores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Cpf.Valor == cpf);
    }

    public async Task<Professor?> ObterPorIdAsync(Guid professorId)
    {
        return await _context.Professores
             .AsNoTracking()
             .FirstOrDefaultAsync(p => p.ProfessorId == professorId);
    }

    public async Task<IEnumerable<Professor>> ObterTodosAsync()
    {
        return await _context.Professores
            .AsNoTracking()
            .ToListAsync();
    }

    public void Remover(Professor professor)
    {
        _context.Professores.Remove(professor);
    }

    public async Task<bool> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}