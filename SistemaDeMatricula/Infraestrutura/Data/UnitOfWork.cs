using Microsoft.EntityFrameworkCore;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Infraestrutura.Repositorios;

namespace SistemaDeMatricula.Infraestrutura.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    // Campos para armazenar as instâncias (Lazy Loading)
    private IRepositorioEstudante _estudantes;

    private IRepositorioTurma _turmas;
    private IRepositorioMatricula _matriculas;
    private IDisciplinaRepositorio _disciplinas;
    private IRepositorioProfessor _professores;
    private IRepositorioNotas _notas;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepositorioEstudante Estudantes => _estudantes ??= new RepositorioEstudante(_context);
    public IRepositorioTurma Turmas => _turmas ??= new RepositorioTurma(_context);

    public IRepositorioMatricula Matriculas => _matriculas ??= new RepositorioMatricula(_context);

    public IRepositorioProfessor Professores => _professores ??= new RepositorioProfessor(_context);

    public IDisciplinaRepositorio Disciplinas => _disciplinas ??= new DisciplinaRepositorio(_context);

    public IRepositorioNotas Notas => _notas ??= new RepositorioNotas(_context);

    private async Task<bool> CommitAsync()
    {
        try
        {
            return await _context.SaveChangesAsync() >= 0; // Se alterou 0 ou 1 linha, é sucesso
        }
        catch (DbUpdateException ex)
        {
            // Logue isso para ver o erro de banco real
            System.Diagnostics.Debug.WriteLine($"ERRO DB: {ex.InnerException?.Message}");
            throw; // Relance a exceção para o teste mostrar o erro real
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    Task<bool> IUnitOfWork.CommitAsync()
    {
        return CommitAsync();
    }
}