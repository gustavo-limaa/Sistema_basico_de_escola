using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Percistencia.Configuracoes;

public class MatriculaConfiguration : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> m)
    {
        m.ToTable("Matriculas");
        m.HasKey(x => x.Id);

        m.HasOne(x => x.Estudante)
         .WithMany(e => e.Matriculas)
         .HasForeignKey(x => x.EstudanteId)
         .OnDelete(DeleteBehavior.Restrict);

        m.HasOne(x => x.Turma)
         .WithMany(t => t.Matriculas)
         .HasForeignKey(x => x.TurmaId)
         .OnDelete(DeleteBehavior.Restrict);

        m.HasIndex(x => new { x.EstudanteId, x.TurmaId }).IsUnique();
        m.HasQueryFilter(m => m.Estudante.Ativo);

        m.Metadata.FindNavigation(nameof(Matricula.Notas))
           .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}