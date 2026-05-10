using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Value_Object;

namespace SistemaDeMatricula.Percistencia.Configuracoes
{
    public class DisciplinaConfiguration : IEntityTypeConfiguration<Disciplina>
    {
        public void Configure(EntityTypeBuilder<Disciplina> d)
        {
            d.ToTable("Disciplinas");
            d.HasKey(x => x.Id);

            d.HasQueryFilter(x => x.Ativo);

            d.Property(x => x.Nome)
             .HasConversion(
                 v => v.Valor,
                 v => new NomeDisciplina(v)
             )
             .HasColumnName("Nome")
             .HasMaxLength(100)
             .IsRequired();

            d.Property(x => x.CargaHoraria)
                .HasConversion(
                    v => v.Valor,
                    v => new CargaHoraria(v)
                )
                .HasColumnName("CargaHoraria")
                .IsRequired();
        }
    }
}