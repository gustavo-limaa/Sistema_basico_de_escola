using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;

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
                 .HasColumnName("DataNascimento")
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
        });

        // 2. PROFESSOR
        modelBuilder.Entity<Professor>(p =>
        {
            p.HasKey(x => x.ProfessorId);

            p.HasQueryFilter(p => p.Ativo);

            p.ComplexProperty(x => x.Salario, s =>
            {
                s.Property(v => v.Valor).HasColumnName("Salario").HasPrecision(18, 2);
                s.Property(v => v.Moeda).HasColumnName("Moeda").HasMaxLength(3);
            });

            p.ComplexProperty(x => x.Email, p =>
                p.Property(v => v.Valor).HasColumnName("Email").HasMaxLength(150));

            p.ComplexProperty(x => x.Cpf, p =>
                p.Property(v => v.Valor).HasColumnName("Cpf").HasMaxLength(11));

            p.ComplexProperty(x => x.NomeCompleto, p =>
                p.Property(v => v.Valor).HasColumnName("Nome").HasMaxLength(80));

            p.ComplexProperty(x => x.Telefone, p =>
                p.Property(v => v.Valor).HasColumnName("Telefone").HasMaxLength(11));

            p.Property(x => x.Categoria).HasColumnName("Categoria").IsRequired();
        });
        // 3. DISCIPLINA

        modelBuilder.Entity<Disciplina>(d =>
        {
            d.HasKey(x => x.DisciplinaId);
            d.HasQueryFilter(x => x.Ativo);

            d.Property(x => x.Nome)
             .HasConversion(
                 v => v.Valor,
                 v => new NomeDisciplina(v)
             )
             .HasMaxLength(100)
             .IsRequired();

            d.Property(x => x.CargaHoraria)
                .HasConversion(
                    v => v.Valor,
                    v => new CargaHoraria(v)
                )
                .IsRequired();
        });

        // 4. TURMA
        modelBuilder.Entity<Turma>(t =>
        {
            t.HasKey(x => x.TurmaId);
            t.Property(x => x.CodigoTurma)
            .HasConversion(
            vo => vo.ValorFormatado,
            stringDoBanco => CodigoTurma.CriarDeString(stringDoBanco)
            )
            .HasColumnName("CodigoTurma")
            .IsRequired();

            t.HasQueryFilter(t => t.Ativo);

            t.HasOne(x => x.Disciplina)
             .WithMany(d => d.Turmas)
             .HasForeignKey(x => x.DisciplinaId)
             .OnDelete(DeleteBehavior.Restrict);

            t.HasOne(p => p.Professor)
             .WithMany()
             .HasForeignKey(p => p.ProfessorId)
             .OnDelete(DeleteBehavior.Restrict);

            t.HasMany(m => m.Matriculas)
             .WithOne(m => m.Turma)
             .HasForeignKey(m => m.TurmaId);
        });

        modelBuilder.Entity<Matricula>(m =>
        {
            m.HasKey(x => x.MatriculaId);

            m.HasOne(x => x.Estudante)
             .WithMany(e => e.Matriculas)
             .HasForeignKey(x => x.EstudanteId)
             .OnDelete(DeleteBehavior.Restrict);

            m.HasOne(x => x.Turma)
             .WithMany(t => t.Matriculas)
             .HasForeignKey(x => x.TurmaId)
             .OnDelete(DeleteBehavior.Restrict);

            m.HasIndex(x => new { x.EstudanteId, x.TurmaId }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}