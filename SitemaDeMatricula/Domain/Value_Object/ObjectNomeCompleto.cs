using System.Text.RegularExpressions;

namespace SitemaDeMatricula.Domain.Value_Objetc;

public partial record ObjectNomeCompleto
{
    public string Valor { get; private init; }

    [GeneratedRegex(@"^[a-zA-ZÀ-ÿ' ]+$")]
    private static partial Regex NomeRegex();

    public ObjectNomeCompleto(string valor)
    {
        var (nome, error) = Criar(valor);
        if (nome is null) throw new ArgumentException(error);
        Valor = nome.Valor;
    }

    private ObjectNomeCompleto()
    {
    }

    private ObjectNomeCompleto(string valor, bool validado) => Valor = valor;

    public static (ObjectNomeCompleto? Nome, string Error) Criar(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, "Nome é obrigatório.");
        var nomeTratado = Regex.Replace(input.Trim(), @"\s+", " ");
        if (nomeTratado.Length < 3 || nomeTratado.Length > 80) return (null, "O nome deve ter entre 3 e 80 caracteres.");
        if (!nomeTratado.Contains(' ')) return (null, "Digite o nome e o sobrenome.");
        if (!NomeRegex().IsMatch(nomeTratado)) return (null, "O nome contém caracteres inválidos.");

        return (new ObjectNomeCompleto(nomeTratado, true), string.Empty);
    }
}