using System.ComponentModel.DataAnnotations.Schema;

namespace SitemaDeMatricula.Domain.Value_Object;

[ComplexType]
public sealed class ObjectDataNascimento
{
    public DateOnly Valor { get; private init; }

    public ObjectDataNascimento(DateOnly valor)
    {
        var (data, error) = Criar(valor);
        if (data is null) throw new ArgumentException(error);
        Valor = data.Valor;
    }

    // Porta dos Fundos
    private ObjectDataNascimento(DateOnly valor, bool validado) => Valor = valor;

    private ObjectDataNascimento()
    { } // Construtor privado para uso interno, se necessário

    public static (ObjectDataNascimento? Data, string Error) Criar(DateOnly dataInput)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        if (dataInput > hoje) return (null, "A data de nascimento não pode ser no futuro.");

        int idade = hoje.Year - dataInput.Year;
        if (dataInput > hoje.AddYears(-idade)) idade--;

        if (idade < 6) return (null, "O aluno deve ter no mínimo 6 anos para ser matriculado.");
        if (idade > 120) return (null, "Data de nascimento inválida (idade limite excedida).");

        return (new ObjectDataNascimento(dataInput, true), string.Empty);
    }
}