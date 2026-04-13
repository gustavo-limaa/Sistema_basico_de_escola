namespace SitemaDeMatricula.Domain.Value_Objetc
{
    using System.Text.RegularExpressions;

    public partial record ObjectNomeCompleto
    {
        public string Valor { get; init; }

        // Regex para permitir letras, acentos e apóstrofos (D'Ávila)
        [GeneratedRegex(@"^[a-zA-ZÀ-ÿ' ]+$")]
        private static partial Regex NomeRegex();

        public ObjectNomeCompleto(string valor) => Valor = valor;

        public static (ObjectNomeCompleto? Nome, string Error) Criar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (null, "Nome é obrigatório.");

            // 1. Limpa espaços extras (Ex: "  João   Silva  " vira "João Silva")
            var nomeTratado = Regex.Replace(input.Trim(), @"\s+", " ");

            // 2. Validação de Tamanho (Mín 3, Máx 80)
            if (nomeTratado.Length < 3 || nomeTratado.Length > 80)
                return (null, "O nome deve ter entre 3 e 80 caracteres.");

            // 3. Validação de Sobrenome (Precisa de pelo menos um espaço)
            if (!nomeTratado.Contains(' '))
                return (null, "Digite o nome e o sobrenome.");

            // 4. Validação de Caracteres Especiais
            if (!NomeRegex().IsMatch(nomeTratado))
                return (null, "O nome contém caracteres inválidos.");

            return (new ObjectNomeCompleto(nomeTratado), string.Empty);
        }
    }
}