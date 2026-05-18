using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Infraestrutura.Repositorios;

namespace SistemaDeMatricula.Infraestrutura.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    // Campos para armazenar as instâncias (Lazy Loading)
    private IRepositorioEstudante _estudantes;

    private IRepositorioTurma _turmas;
    private IRepositorioMatricula _matriculas;
    private IDisciplinaRepositorio _disciplinas; // Novo!
    private IRepositorioProfessor _professores;  // Novo!

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepositorioEstudante Estudantes => _estudantes ??= new RepositorioEstudante(_context);
    public IRepositorioTurma Turmas => _turmas ??= new RepositorioTurma(_context);

    public IRepositorioMatricula Matriculas => _matriculas ??= new RepositorioMatricula(_context);

    public IRepositorioProfessor Professores => _professores ??= new RepositorioProfessor(_context);

    public IDisciplinaRepositorio Disciplinas => _disciplinas ??= new DisciplinaRepositorio(_context);

    public async Task<bool> CommitAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}