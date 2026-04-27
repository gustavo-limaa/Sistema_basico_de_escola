using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SitemaDeMatricula.Domain;

[DebuggerDisplay("{Sucesso ? \"✅ OK\" : \"❌ Falha\"}: {Mensagem}")]
public class Result<T>
{
    [MemberNotNullWhen(true, nameof(Dados))]
    [MemberNotNullWhen(false, nameof(Mensagem))]
    public bool Sucesso { get; private set; }

    public T? Dados { get; private set; }
    public string Mensagem { get; private set; }

    protected Result(bool sucesso, T? dados, string mensagem)
    {
        Sucesso = sucesso;
        Dados = dados;
        Mensagem = mensagem;
    }

    public static Result<T> Ok(T dados, string mensagem = "Operação realizada com sucesso.")
        => new(true, dados, mensagem);

    public static Result<T> SemConteudo(string mensagem = "Operação realizada com sucesso.")
        => new(true, default, mensagem);

    public static Result<T> Falha(string mensagem)
        => new(false, default, mensagem);
}