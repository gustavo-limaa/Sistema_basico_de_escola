using SistemaDeMatricula.Domain.Uteis;
using System.Reflection.PortableExecutable;

namespace SistemaDeMatricula.Domain.Modelos
{
    public class Nota : ModeloMain
    {
        public Guid id { get; private set; }

        public double valor { get; private set; }
        public TipoImportancia importancia { get; private set; }
        public string descricao { get; private set; } = null!;
        public DateTime data { get; private set; }
        public CategoriaAvaliacao

        public Guid disciplinaId { get; private set; }
        public Disciplina disciplina { get; private set; } = null!;

        public Guid alunoId { get; private set; }
        public Estudante estudante { get; private set; } = null!;

        public Guid MatriculaId { get; private set; }
        public Matricula Matricula { get; private set; } = null!;
    }
}