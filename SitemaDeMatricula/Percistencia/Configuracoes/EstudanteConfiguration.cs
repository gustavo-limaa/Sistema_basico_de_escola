using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Percistencia.Configuracoes;

public class EstudanteConfiguration : IEntityTypeConfiguration<Estudante>
{
    public void Configure(EntityTypeBuilder<Estudante> builder)
    {
        builder.ToTable("Estudantes");
        builder.HasKey(x => x.Id);

        builder.ComplexProperty(x => x.DataNascimento,
            p => p.Property(v => v.Valor).HasColumnName("DataNascimento").IsRequired());

        builder.ComplexProperty(x => x.Email, p =>
            p.Property(v => v.Valor).HasColumnName("Email").HasMaxLength(150));

        builder.ComplexProperty(x => x.Cpf, p =>
            p.Property(v => v.Valor).HasColumnName("Cpf").HasMaxLength(11));

        builder.ComplexProperty(x => x.NomeCompleto, p =>
            p.Property(v => v.Valor).HasColumnName("NomeCompleto").HasMaxLength(80));

        builder.ComplexProperty(x => x.Telefone, p =>
            p.Property(v => v.Valor).HasColumnName("Telefone").HasMaxLength(11));

        builder.HasQueryFilter(x => x.Ativo);
    }
}