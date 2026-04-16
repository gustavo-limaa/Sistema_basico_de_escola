namespace SitemaDeMatricula.Domain.Value_Object;

public record NomeDisciplina
{
    public string Valor { get; }

    public NomeDisciplina(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("O nome da disciplina não pode ser vazio.");

        if (valor.Length < 3 || valor.Length > 100)
            throw new ArgumentException("O nome da disciplina deve ter entre 3 e 100 caracteres.");

        // Aqui você pode até colocar um .ToUpper() ou .Trim() para padronizar
        Valor = valor.Trim();
    }

    // Conversão implícita para facilitar a vida: permite usar o VO como string
    public static implicit operator string(NomeDisciplina nome) => nome.Valor;
    public static implicit operator NomeDisciplina(string valor) => new NomeDisciplina(valor);
}