using System.Text.RegularExpressions;

namespace SitemaDeMatricula.Domain.Value_Objetc;

public partial record ObjectEmail
{
    public string Valor { get; private init; }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    // 1. Porta da Frente
    public ObjectEmail(string valor)
    {
        var (email, error) = Criar(valor);
        if (email == null) throw new ArgumentException(error);
        Valor = email.Valor;
    }
    private ObjectEmail()
    {
    }

    // 2. Porta dos Fundos
    private ObjectEmail(string valor, bool validado) => Valor = valor;

    // 3. Factory Method
    public static (ObjectEmail? Email, string Error) Criar(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, "O e-mail não pode ser vazio.");
        var valorTratado = input.Trim().ToLower();
        if (!EmailRegex().IsMatch(valorTratado)) return (null, "O formato do e-mail é inválido.");

        return (new ObjectEmail(valorTratado, true), string.Empty);
    }
}