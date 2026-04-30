namespace SitemaDeMatricula.Domain.Value_Object;

public sealed class CodigoTurma
{
    // Usamos tipos primitivos aqui para representar os pedaços do código
    public string Sigla { get; private init; }

    public int Ano { get; private init; }
    public int Semestre { get; private init; }
    public int Numero { get; private init; }

    // O Banco de Dados e o Mapper usarão esta propriedade
    public string ValorFormatado => $"{Sigla}-{Ano}-{Semestre}-{Numero:D3}";

    public CodigoTurma(string sigla, int ano, int semestre, int numero)
    {
        Sigla = sigla;
        Ano = ano;
        Semestre = semestre;
        Numero = numero;
    }

    public static Result<CodigoTurma> Criar(string sigla, int ano, int semestre, int numero)
    {
        if (string.IsNullOrWhiteSpace(sigla) || sigla.Length != 3)
            return Result<CodigoTurma>.Falha("A sigla deve ter exatamente 3 caracteres.");

        if (semestre != 1 && semestre != 2)
            return Result<CodigoTurma>.Falha("O semestre deve ser 1 ou 2.");

        if (ano < DateTime.Now.Year)
            return Result<CodigoTurma>.Falha("O ano não pode ser inferior ao atual.");

        // Usando o seu método estático Ok
        var novoCodigo = new CodigoTurma(sigla.ToUpper(), ano, semestre, numero);
        return Result<CodigoTurma>.Ok(novoCodigo);
    }

    // Dentro da classe CodigoTurma
    public static CodigoTurma CriarDeString(string valorCompleto)
    {
        // Divide a string nos traços: ["MAT", "2026", "1", "001"]
        var partes = valorCompleto.Split('-');

        if (partes.Length != 4)
            throw new Exception("Formato de código de turma inválido no banco de dados.");

        var sigla = partes[0];
        var ano = int.Parse(partes[1]);
        var semestre = int.Parse(partes[2]);
        var numero = int.Parse(partes[3]);

        // Como os dados já vieram do banco (supostamente válidos),
        // podemos usar o construtor privado diretamente
        return new CodigoTurma(sigla, ano, semestre, numero);
    }

    // Construtor necessário para o EF Core (pode ser privado)
    private CodigoTurma()
    { }
}