namespace SistemaDeMatricula.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositórios como propriedades
    IRepositorioEstudante Estudantes { get; }

    IRepositorioTurma Turmas { get; }
    IRepositorioMatricula Matriculas { get; }
    IRepositorioProfessor Professores { get; }
    IDisciplinaRepositorio Disciplinas { get; }

    // O único lugar que decide QUANDO salvar
    Task<bool> CommitAsync();
}