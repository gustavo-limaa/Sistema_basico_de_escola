namespace SistemaDeMatricula.Domain.Modelos
{
    using Microsoft.AspNetCore.Components.Web;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public sealed class Matricula : ModeloMain
    {
        public DateTime DataMatricula { get; private set; } = DateTime.UtcNow;

        public Guid EstudanteId { get; private set; }
        public Estudante Estudante { get; private set; } = null!;

        public Guid TurmaId { get; private set; }
        public Turma Turma { get; private set; } = null!;

        public Matricula(Guid estudanteId, Guid turmaId) : base()
        {
            EstudanteId = estudanteId;
            TurmaId = turmaId;
            DataMatricula = DateTime.UtcNow;
        }

        protected Matricula()
        { } // Necessário para o EF Core

        public void Desativar() => Ativo = false;
    }
}