using SistemaDeMatricula.Domain.Uteis;

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
            if (valor < 0 || valor > 10) throw new DomainException("Nota não pode ser negativa É nem menor q 0 ou maior que 10");
            if (string.IsNullOrWhiteSpace(descricao)) throw new DomainException("Descrição não pode ser vazia");
            if (matriculaId == Guid.Empty) throw new DomainException("MatriculaId não pode ser vazio");
            if (!Enum.IsDefined(typeof(TipoImportancia), importancia)) throw new DomainException("Tipo de importância inválido");
            if (dataEmissao == DateTime.MinValue) throw new DomainException("Data de emissão inválida");
            if (!Enum.IsDefined(typeof(CategoriaAvaliacao), categoria)) throw new DomainException("Categoria de avaliação inválida");
            MatriculaId = matriculaId;
            Importancia = importancia;
            Categoria = categoria;
            Valor = valor;
            Descricao = descricao;
            DataEmissao = dataEmissao;
        }

        public void AtualizarDados(double valor, string descricao, TipoImportancia importancia, CategoriaAvaliacao categoria)
        {
            if (valor < 0 || valor > 10) throw new DomainException("Nota não pode ser negativa É nem menor q 0 ou maior que 10");
            if (string.IsNullOrWhiteSpace(descricao)) throw new DomainException("Descrição não pode ser vazia");
            if (!Enum.IsDefined(typeof(TipoImportancia), importancia)) throw new DomainException("Tipo de importância inválido");
            if (!Enum.IsDefined(typeof(CategoriaAvaliacao), categoria)) throw new DomainException("Categoria de avaliação inválida");
            if (DataEmissao == DateTime.MinValue) throw new DomainException("Data de emissão inválida");
            Valor = valor;
            Descricao = descricao;
            Importancia = importancia;
            Categoria = categoria;
        }
    }
}