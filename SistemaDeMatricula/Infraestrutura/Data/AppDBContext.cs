using Microsoft.EntityFrameworkCore;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Infraestrutura.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Suas tabelas
    public DbSet<Estudante> Estudantes { get; set; }

    public DbSet<Professor> Professores { get; set; }

    public DbSet<Disciplina> Disciplinas { get; set; }
    public DbSet<Turma> Turmas { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }
    public DbSet<Nota> notas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}