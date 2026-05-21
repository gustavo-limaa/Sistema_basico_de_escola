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

        public Nota(Guid id, Guid matriculaId, TipoImportancia importancia, CategoriaAvaliacao categoria, double valor, string descricao, DateTime dataEmissao)
        {
            Id = id;
            MatriculaId = matriculaId;
            Importancia = importancia;
            Categoria = categoria;
            Valor = valor;
            Descricao = descricao;
            DataEmissao = dataEmissao;
        }
    }
}