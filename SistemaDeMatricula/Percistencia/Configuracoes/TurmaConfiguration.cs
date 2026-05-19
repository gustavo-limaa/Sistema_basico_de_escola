using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Value_Object;

namespace SistemaDeMatricula.Percistencia.Configuracoes;

public class TurmaConfiguration : IEntityTypeConfiguration<Turma>
{
    public void Configure(EntityTypeBuilder<Turma> t)
    {
        t.ToTable("Turmas");
        t.HasKey(x => x.Id);

        t.Property(x => x.CodigoTurma)
         .HasConversion(
             vo => vo.ValorFormatado,
             stringDoBanco => CodigoTurma.CriarDeString(stringDoBanco)
         )
         .HasColumnName("CodigoTurma")
         .IsRequired();
        t.Property(x => x.CapacidadeMaxima);

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
    }
}