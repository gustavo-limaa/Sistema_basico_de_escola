using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using System.Reflection.PortableExecutable;

namespace SistemaDeMatricula.Domain.Modelos
{
    public class Nota : ModeloMain
    {
        public double Valor { get; private set; }
        public TipoImportancia Importancia { get; private set; }
        public string Descricao { get; private set; } = null!;

        public CategoriaAvaliacao Categoria { get; private set; }

        public DateTime DataEmissao { get; private set; } = DateTime.UtcNow;

        public Guid MatriculaId { get; private set; }
        public Matricula Matricula { get; private set; } = null!;

        public Nota(Guid matriculaId, TipoImportancia importancia, CategoriaAvaliacao categoria, double valor, string descricao, DateTime dataEmissao) : base()
        {
            MatriculaId = matriculaId;
            Importancia = importancia;
            Categoria = categoria;
            Valor = valor;
            Descricao = descricao;
            DataEmissao = dataEmissao;
        }

        public void AtualizarDados(double valor, string descricao, TipoImportancia importancia, CategoriaAvaliacao categoria)
        {
            if (valor < 0) throw new ArgumentException("Nota não pode ser negativa");
            Valor = valor;
            Descricao = descricao;
            Importancia = importancia;
            Categoria = categoria;
        }
    }
}