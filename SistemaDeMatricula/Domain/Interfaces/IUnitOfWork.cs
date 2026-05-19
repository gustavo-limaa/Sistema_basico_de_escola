namespace SistemaDeMatricula.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepositorioEstudante Estudantes { get; }

    IRepositorioTurma Turmas { get; }
    IRepositorioMatricula Matriculas { get; }
    IRepositorioProfessor Professores { get; }
    IDisciplinaRepositorio Disciplinas { get; }

    Task<bool> CommitAsync();
}