using System.Text.RegularExpressions;

namespace SitemaDeMatricula.Domain.Value_Objetc;

public partial record ObjectTelefone
{
    public string Valor { get; private init; }

    [GeneratedRegex(@"^\d{10,11}$")]
    private static partial Regex TelefoneRegex();

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex ApenasNumerosRegex();

    public ObjectTelefone(string valor)
    {
        var (telefone, error) = Criar(valor);
        if (telefone is null) throw new ArgumentException(error);
        Valor = telefone.Valor;
    }
    private ObjectTelefone()
    {
    }

    // Porta dos Fundos
    private ObjectTelefone(string valor, bool validado) => Valor = valor;

    public static (ObjectTelefone? Telefone, string Error) Criar(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, "O telefone é obrigatório.");
        var numeros = ApenasNumerosRegex().Replace(input, "");
        if (!TelefoneRegex().IsMatch(numeros)) return (null, "Telefone inválido. Deve conter DDD + número (10 ou 11 dígitos).");

        return (new ObjectTelefone(numeros, true), string.Empty);
    }

    public string Formatar() => Valor.Length == 11
        ? long.Parse(Valor).ToString(@"(00) 00000-0000")
        : long.Parse(Valor).ToString(@"(00) 0000-0000");
}