namespace SistemaDeMatricula.Domain.Modelos
{
    using SistemaDeMatricula.Domain.Uteis;

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

        //Estado do estudante
        private List<Nota> _notas = new List<Nota>();

        public IReadOnlyCollection<Nota> Notas => _notas.AsReadOnly();
        public bool Aprovado => CalcularMediaFinal() >= 6.0;
        public bool Recuperacao => CalcularMediaFinal() >= 4.0 && CalcularMediaFinal() < 6.0;

        public bool Reprovado => CalcularMediaFinal() < 4.0;

        public Double MediaFinal => CalcularMediaFinal();

        protected Matricula()
        { } // Necessário para o EF Core

        public void Desativar() => Ativo = false;

        public double CalcularMediaFinal()
        {
            if (!_notas.Any()) return 0.0;

            double somaDasNotasComPeso = _notas.Sum(n => n.Valor * ObterPeso(n.Importancia));
            int somaDosPesos = _notas.Sum(n => ObterPeso(n.Importancia));

            return somaDasNotasComPeso / somaDosPesos;
        }

        private int ObterPeso(TipoImportancia importancia) => importancia switch
        {
            TipoImportancia.Alta => 3,
            TipoImportancia.Media => 2,
            _ => 1
        };

        public Nota AdicionarNota(double valor, string descricao, TipoImportancia importancia, CategoriaAvaliacao categoria)
        {
            if (valor < 0) throw new DomainException("Nota não pode ser negativa");

            var novaNota = new Nota(this.Id, importancia, categoria, valor, descricao, DateTime.UtcNow);
            _notas.Add(novaNota);

            return novaNota;
        }
    }
}