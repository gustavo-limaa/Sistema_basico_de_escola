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
        // 3. DISCIPLINA (Versão Limpa)
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

            // Note que aqui NÃO colocamos o HasMany(t => t.Turmas).
            // Deixamos a configuração lá no bloco da Turma (item 4),
            // que já faz o link com d.Turmas corretamente.
        });

        // 4. TURMA
        modelBuilder.Entity<Turma>(t =>
        {
            t.HasKey(x => x.TurmaId);
            t.Property(x => x.CodigoTurma)
            .HasConversion(
            vo => vo.ValorFormatado, // Converte para string ao salvar no banco
            stringDoBanco => CodigoTurma.CriarDeString(stringDoBanco) // Converte para VO ao ler do banco
            )
            .HasColumnName("CodigoTurma") // Nome da coluna no SQL
            .IsRequired();

            t.HasQueryFilter(t => t.Ativo);

            // 1. Relacionamento com Disciplina (Essencial!)
            // Dentro do mapeamento da Turma no AppDbContext
            t.HasOne(x => x.Disciplina)
             .WithMany(d => d.Turmas) // Conecta com a lista que já existe na Disciplina
             .HasForeignKey(x => x.DisciplinaId)
             .OnDelete(DeleteBehavior.Restrict);

            // 2. Relacionamento com Professor
            t.HasOne(p => p.Professor)
             .WithMany() // Se o Professor tiver List<Turma>, coloque p => p.Turmas
             .HasForeignKey(p => p.ProfessorId)
             .OnDelete(DeleteBehavior.Restrict);

            // 3. Relacionamento com Matrículas (Um-para-Muitos)
            t.HasMany(m => m.Matriculas)
             .WithOne(m => m.Turma)
             .HasForeignKey(m => m.TurmaId);
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