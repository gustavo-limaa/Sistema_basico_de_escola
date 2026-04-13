namespace SitemaDeMatricula.Domain.Modelos
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Matricula
    {
        [Key]
        public Guid MatriculaId { get; private set; } = Guid.NewGuid();

        [Required]
        public DateTime DataMatricula { get; private set; } = DateTime.Now;

        // Chaves Estrangeiras
        [Required]
        public Guid EstudanteId { get; private set; }

        [ForeignKey("EstudanteId")]
        public Estudante Estudante { get; private set; } = null!;

        [Required]
        public Guid TurmaId { get; private set; }

        [ForeignKey("TurmaId")]
        public Turma Turma { get; private set; } = null!;

        // Construtor para garantir a criação correta
        public Matricula(Guid estudanteId, Guid turmaId)
        {
            EstudanteId = estudanteId;
            TurmaId = turmaId;
            DataMatricula = DateTime.Now;
        }

        protected Matricula()
        { } // Necessário para o EF Core
    }
}