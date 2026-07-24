using Microsoft.EntityFrameworkCore;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SistemaDeMatricula.Infraestrutura.Repositorios;

public class RepositorioEstudante : IRepositorioEstudante
{
    private readonly AppDbContext _context;

    public RepositorioEstudante(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Estudante>> ObterTodosAsync()
    {
        return await _context.Estudantes
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estudante?> ObterPorIdAsync(Guid estudanteId)
    {
        return await _context.Estudantes
            .FirstOrDefaultAsync(e => e.Id == estudanteId);
    }

    public async Task AdicionarAsync(Estudante estudante)
    {
        await _context.Estudantes.AddAsync(estudante);
    }

    public void Atualizar(Estudante estudante)
    {
        _context.Estudantes.Update(estudante);
    }

    public void Remover(Estudante estudante)
    {
        var estudanteNoBanco = _context.Estudantes
            .FirstOrDefault(e => e.Id == estudante.Id);

        if (estudanteNoBanco == null || !estudanteNoBanco.Ativo)
            return;

        estudanteNoBanco.DesativarEstudante();

        _context.Estudantes.Update(estudanteNoBanco);
    }

    public async Task<bool> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExisteCpfAsync(string cpf)
    {
        return await _context.Estudantes
        .IgnoreQueryFilters() // 🔥 Adicionando isso aqui!
        .AnyAsync(e => e.Cpf.Valor == cpf);
    }

    public async Task<bool> ExisteMatriculaAsync(Guid estudanteId)
    {
        return await _context.Estudantes.AnyAsync(m => m.Id == estudanteId);
    }

    public Task<bool> ExisteEmailAsync(string email, Guid id)
    {
        return _context.Estudantes.AnyAsync(e => e.Email.Valor == email && e.Id != id);
    }

    public async Task<Estudante?> ObterPorCpfAsync(string cpf)
    {
        // 🎯 O PULO DO GATO: await + FirstOrDefaultAsync
        return await _context.Estudantes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Cpf.Valor == cpf);
    }
}