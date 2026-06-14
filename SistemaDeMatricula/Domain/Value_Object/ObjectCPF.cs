namespace SitemaDeMatricula.Domain.Value_Objetc
{
    using SistemaDeMatricula.Domain.Uteis;
    using System.Text.RegularExpressions;

    public partial record ObjectCPF
    {
        public string Valor { get; private init; }

        [GeneratedRegex(@"[^\d]")]
        private static partial Regex ApenasNumerosRegex();

        public ObjectCPF(string valor)
        {
            var (cpf, error) = Criar(valor);

            if (cpf is null)
                throw new DomainException(error);

            Valor = cpf.Valor;
        }

        private ObjectCPF(string valor, bool validado)
        {
            Valor = valor;
        }

        private ObjectCPF()
        {
        }

        public static (ObjectCPF? Cpf, string Error) Criar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (null, "CPF é obrigatório.");

            var cpfLimpo = ApenasNumerosRegex().Replace(input, "");

            if (cpfLimpo.Length != 11 || TodosNumerosIguais(cpfLimpo))
                return (null, "CPF deve conter 11 dígitos válidos.");

            // COLOCANDO A CHAMADA DO VALIDADOR AQUI!
            if (!ValidarDigitos(cpfLimpo))
                return (null, "CPF matematicamente inválido.");

            return (new ObjectCPF(cpfLimpo, true), string.Empty);
        }
        private static bool TodosNumerosIguais(string cpf) =>
            cpf.All(c => c == cpf[0]);

        private static bool ValidarDigitos(string cpf)
        {
            int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

            // Primeiro dígito
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (cpf[i] - '0') * multiplicador1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            // Segundo dígito (Agora englobando o digito1 no loop perfeitamente)
            soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (cpf[i] - '0') * multiplicador2[i];

            soma += digito1 * multiplicador2[9]; // Peso 2

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return (cpf[9] - '0') == digito1 && (cpf[10] - '0') == digito2;
        }
        public string Formatar() =>
            long.Parse(Valor).ToString(@"000\.000\.000\-00");
    }
}