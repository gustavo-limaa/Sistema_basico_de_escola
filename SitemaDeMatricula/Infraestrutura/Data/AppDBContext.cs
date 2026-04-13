using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.InfraEstrutura.Data;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. ESTUDANTE
        modelBuilder.Entity<Estudante>(e =>
        {
            e.HasKey(x => x.EstudanteId);

            e.ComplexProperty(x => x.DataNascimento, p =>
            {
                p.Property(v => v.Valor)
                 .HasColumnName("DataNascimento") // Nome da coluna no MySQL
                 .IsRequired();
            });

            e.ComplexProperty(x => x.Email, p =>
                p.Property(v => v.Valor).HasColumnName("Email").HasMaxLength(150));

            e.ComplexProperty(x => x.Cpf, p =>
                p.Property(v => v.Valor).HasColumnName("Cpf").HasMaxLength(11));

            e.ComplexProperty(x => x.NomeCompleto, p =>
                p.Property(v => v.Valor).HasColumnName("NomeCompleto").HasMaxLength(80));

            e.ComplexProperty(x => x.Telefone, p =>
                p.Property(v => v.Valor).HasColumnName("Telefone").HasMaxLength(11));
        }); // <--- FECHA Estudante aqui

        // 2. PROFESSOR
        modelBuilder.Entity<Professor>(p =>
        {
            p.HasKey(x => x.ProfessorId);

            p.ComplexProperty(x => x.Salario, s =>
            {
                s.Property(v => v.Valor).HasColumnName("Salario").HasPrecision(18, 2);
                s.Property(v => v.Moeda).HasColumnName("Moeda").HasMaxLength(3);
            });

            p.ComplexProperty(x => x.Email, p =>
                p.Property(v => v.Valor).HasColumnName("Email").HasMaxLength(150));

            p.ComplexProperty(x => x.Cpf, p =>
                p.Property(v => v.Valor).HasColumnName("Cpf").HasMaxLength(11));

            p.ComplexProperty(x => x.Nome, p =>
                p.Property(v => v.Valor).HasColumnName("Nome").HasMaxLength(80));

            p.ComplexProperty(x => x.Telefone, p =>
                p.Property(v => v.Valor).HasColumnName("Telefone").HasMaxLength(11));

            p.Property(x => x.Categoria).HasColumnName("Categoria").IsRequired();
        }); // <--- FECHA Professor aqui

        // 3. DISCIPLINA
        modelBuilder.Entity<Disciplina>(d =>
        {
            d.HasKey(x => x.DisciplinaId);
            d.HasMany(t => t.Turmas)
             .WithOne(t => t.Disciplina)
             .HasForeignKey(t => t.DisciplinaId);
        });

        // 4. TURMA
        modelBuilder.Entity<Turma>(t =>
        {
            t.HasKey(x => x.TurmaId);

            t.HasMany(m => m.Matriculas)
             .WithOne(m => m.Turma)
             .HasForeignKey(m => m.TurmaId);

            t.HasOne(p => p.Professor)
             .WithMany()
             .HasForeignKey(p => p.ProfessorId);
        });

        // 5. MATRICULA (A tabela N:N que faltava mapear)

        modelBuilder.Entity<Matricula>(m =>
        {
            m.HasKey(x => x.MatriculaId);

            // Ligação com Estudante (1 Estudante -> Várias Matrículas)
            m.HasOne(x => x.Estudante)
             .WithMany(e => e.Matriculas) // Plural aqui!
             .HasForeignKey(x => x.EstudanteId)
             .OnDelete(DeleteBehavior.Restrict);

            // Ligação com Turma (1 Turma -> Várias Matrículas)
            m.HasOne(x => x.Turma)
             .WithMany(t => t.Matriculas) // Plural aqui!
             .HasForeignKey(x => x.TurmaId)
             .OnDelete(DeleteBehavior.Restrict);

            m.HasIndex(x => new { x.EstudanteId, x.TurmaId }).IsUnique();
        });

        base.OnModelCreating(modelBuilder); // Sempre por último
    }
}