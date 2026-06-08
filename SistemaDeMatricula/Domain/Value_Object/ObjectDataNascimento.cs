using SistemaDeMatricula.Domain.Uteis;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeMatricula.Domain.Value_Object;

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
        if (dataInput > hoje) throw new DomainException("A data de nascimento não pode ser no futuro.");

        int idade = hoje.Year - dataInput.Year;
        if (dataInput > hoje.AddYears(-idade)) idade--;

        if (idade < 6) throw new DomainException("O aluno deve ter no mínimo 6 anos para ser matriculado.");
        if (idade > 120) throw new DomainException("Data de nascimento inválida (idade limite excedida).");

        return (new ObjectDataNascimento(dataInput, true), string.Empty);
    }
}