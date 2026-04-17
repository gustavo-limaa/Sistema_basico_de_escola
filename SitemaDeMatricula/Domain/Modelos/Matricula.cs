namespace SitemaDeMatricula.Domain.Modelos
{
    using Microsoft.AspNetCore.Components.Web;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Matricula
    {
        [Key]
        public Guid MatriculaId { get; private set; } = Guid.NewGuid();

        [Required]
        public DateTime DataMatricula { get; private set; } = DateTime.UtcNow;

        // Chaves Estrangeiras
        [Required]
        public Guid EstudanteId { get; private set; }

        [ForeignKey("EstudanteId")]
        public Estudante Estudante { get; private set; } = null!;

        [Required]
        public Guid TurmaId { get; private set; }

        [ForeignKey("TurmaId")]
        public Turma Turma { get; private set; } = null!;

        public bool Ativo { get; private set; } = true;

        // Construtor para garantir a criação correta
        public Matricula(Guid estudanteId, Guid turmaId)
        {
            MatriculaId = Guid.NewGuid();
            EstudanteId = estudanteId;
            TurmaId = turmaId;
            DataMatricula = DateTime.UtcNow;
            Ativo = true;
        }

        protected Matricula()
        { } // Necessário para o EF Core

        public void Desativar() => Ativo = false;
    }
}