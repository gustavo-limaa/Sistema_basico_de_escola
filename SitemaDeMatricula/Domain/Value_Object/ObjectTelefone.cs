using System.Text.RegularExpressions;

namespace SitemaDeMatricula.Domain.Value_Objetc;

public partial record ObjectTelefone
{
    public string Valor { get; init; }

    // Regex que aceita apenas números e verifica se tem entre 10 e 11 dígitos
    [GeneratedRegex(@"^\d{10,11}$")]
    private static partial Regex TelefoneRegex();

    // Regex para remover qualquer caractere que não seja número (limpeza)
    [GeneratedRegex(@"[^\d]")]
    private static partial Regex ApenasNumerosRegex();

    public ObjectTelefone(string valor) => Valor = valor;

    public static (ObjectTelefone? Telefone, string Error) Criar(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (null, "O telefone é obrigatório.");

        // Limpa parênteses, pontos, traços e espaços
        var numeros = ApenasNumerosRegex().Replace(input, "");

        if (!TelefoneRegex().IsMatch(numeros))
            return (null, "Telefone inválido. Deve conter DDD + número (10 ou 11 dígitos).");

        return (new ObjectTelefone(numeros), string.Empty);
    }

    // Método auxiliar para formatar na hora de exibir
    public string Formatar()
    {
        return Valor.Length == 11
            ? long.Parse(Valor).ToString(@"(00) 00000-0000")
            : long.Parse(Valor).ToString(@"(00) 0000-0000");
    }
}