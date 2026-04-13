namespace SitemaDeMatricula.Domain.Value_Objetc
{
    using System.Text.RegularExpressions;

    public partial record ObjectCPF
    {
        public string Valor { get; init; }

        [GeneratedRegex(@"[^\d]")]
        private static partial Regex ApenasNumerosRegex();

        public ObjectCPF(string valor) => Valor = valor;

        public static (ObjectCPF? Cpf, string Error) Criar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (null, "CPF é obrigatório.");

            var cpfLimpo = ApenasNumerosRegex().Replace(input, "");

            if (cpfLimpo.Length != 11 || TodosNumerosIguais(cpfLimpo))
                return (null, "CPF deve conter 11 dígitos válidos.");

            if (!ValidarDigitos(cpfLimpo))
                return (null, "CPF matematicamente inválido.");

            return (new ObjectCPF(cpfLimpo), string.Empty);
        }

        private static bool TodosNumerosIguais(string cpf) =>
            cpf.All(c => c == cpf[0]);

        private static bool ValidarDigitos(string cpf)
        {
            // Algoritmo simplificado de validação de CPF
            int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

            string tempCpf = cpf[..9];
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            string digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }

        public string Formatar() =>
            long.Parse(Valor).ToString(@"000\.000\.000\-00");
    }
}