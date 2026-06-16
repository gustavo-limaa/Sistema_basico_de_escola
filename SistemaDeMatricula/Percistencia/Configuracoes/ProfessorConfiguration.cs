using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Percistencia.Configuracoes
{
    public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
    {
        public void Configure(EntityTypeBuilder<Professor> p)
        {
            p.ToTable("Professores");
            p.HasKey(x => x.Id);

            p.HasQueryFilter(x => x.Ativo);

            //p.HasIndex(x => x.Cpf.Valor).IsUnique().HasDatabaseName("IX_Professor_Cpf");

            p.ComplexProperty(x => x.Salario, s =>
            {
                s.Property(v => v.Valor)
                 .HasColumnName("Salario")
                 .HasPrecision(18, 2);
                s.Property(v => v.Moeda)
                 .HasColumnName("Moeda")
                 .HasMaxLength(3);
            });

            p.ComplexProperty(x => x.Email, prop =>
                prop.Property(v => v.Valor).HasColumnName("Email").HasMaxLength(150));

            p.ComplexProperty(x => x.Cpf, prop =>
                prop.Property(v => v.Valor).HasColumnName("Cpf").HasMaxLength(11)
                );

            p.ComplexProperty(x => x.NomeCompleto, prop =>
                prop.Property(v => v.Valor).HasColumnName("Nome").HasMaxLength(80));

            p.ComplexProperty(x => x.Telefone, prop =>
                prop.Property(v => v.Valor).HasColumnName("Telefone").HasMaxLength(11));

            p.Property(x => x.Categoria)
             .HasColumnName("Categoria");
        }
    }
}