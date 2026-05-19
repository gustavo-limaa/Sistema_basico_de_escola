namespace SistemaDeMatricula.Domain.Value_Object
{
    public record ValorMonetario // Usar 'record' é excelente para Value Objects (imutabilidade)
    {
        public decimal Valor { get; private init; }
        public string Moeda { get; private init; } = "BRL"; // Valor padrão para sua realidade atual

        public ValorMonetario() { } // Necessário para o EF Core

        public ValorMonetario(decimal valor, string moeda = "BRL")
        {
            if (valor < 0)
                throw new ArgumentException("O valor monetário não pode ser negativo.");

            Valor = valor;
            Moeda = moeda;
        }

        // Curiosidade: Sobrecarga de operador para facilitar cálculos
        public static ValorMonetario operator +(ValorMonetario a, ValorMonetario b)
            => new(a.Valor + b.Valor, a.Moeda);
    }
}