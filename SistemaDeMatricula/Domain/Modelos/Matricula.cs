namespace SistemaDeMatricula.Domain.Modelos
{
    using Microsoft.AspNetCore.Components.Web;
    using SistemaDeMatricula.Domain.Uteis;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public sealed class Matricula : ModeloMain
    {
        public DateTime DataMatricula { get; private set; } = DateTime.UtcNow;

        public Guid EstudanteId { get; private set; }
        public Estudante Estudante { get; private set; } = null!;

        public Guid TurmaId { get; private set; }
        public Turma Turma { get; private set; } = null!;

        //Estado do estudante
        private List<Nota> _notas = new List<Nota>();

        public IReadOnlyCollection<Nota> Notas => _notas.AsReadOnly();
        public bool Aprovado => CalcularMediaFinal() >= 6.0;
        public bool Recuperacao => CalcularMediaFinal() >= 4.0 && CalcularMediaFinal() < 6.0;
        public bool Reprovado => CalcularMediaFinal() < 4.0;
        public Double MediaFinal => CalcularMediaFinal();

        public Matricula(Guid estudanteId, Guid turmaId) : base()
        {
            EstudanteId = estudanteId;
            TurmaId = turmaId;
            DataMatricula = DateTime.UtcNow;
        }

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
            _ => 1 // O Default ou Baixa valem 1
        };
    }
}