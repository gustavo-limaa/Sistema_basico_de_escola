using System.Text.RegularExpressions;

namespace SitemaDeMatricula.Domain.Value_Objetc;

public partial record ObjectEmail
{
    public string Valor { get; init; } // 'init' garante imutabilidade após a criação

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    // Construtor privado: Ninguém cria um e-mail sem passar pela porta principal
    public ObjectEmail(string valor) => Valor = valor;

    public static (ObjectEmail? Email, string Error) Criar(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (null, "O e-mail não pode ser vazio.");

        var valorTratado = input.Trim().ToLower();

        if (!EmailRegex().IsMatch(valorTratado))
            return (null, "O formato do e-mail é inválido.");

        return (new ObjectEmail(valorTratado), string.Empty);
    }
}