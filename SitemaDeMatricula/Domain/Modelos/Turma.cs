namespace SitemaDeMatricula.Domain.Modelos
{
    public class Turma
    {
        public Guid TurmaId { get; private set; } = Guid.NewGuid();
        public string CodigoTurma { get; private set; } // Ex: "MAT-2026-A"

        // Relacionamentos
        public Guid ProfessorId { get; private set; }

        public Professor Professor { get; private set; }

        public Guid DisciplinaId { get; private set; }
        public Disciplina Disciplina { get; private set; }

        // Uma turma tem uma lista de matrículas (Estudantes)
        public List<Matricula> Matriculas { get; private set; } = new();

        public ICollection<Estudante> Estudantes { get; private set; } = new List<Estudante>();

        public Turma(string codigo, Guid professorId, Guid disciplinaId)
        {
            CodigoTurma = codigo;
            ProfessorId = professorId;
            DisciplinaId = disciplinaId;
        }

        public Turma()
        {
        }
    }
}