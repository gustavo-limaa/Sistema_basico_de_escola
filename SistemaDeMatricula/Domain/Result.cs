using System.Diagnostics.CodeAnalysis;

namespace SistemaDeMatricula.Domain;

public enum TipoErro
{
    Validacao,
    NaoEncontrado,
    Conflito,
    Inesperado
}

public class Result<T>
{
    [MemberNotNullWhen(true, nameof(Dados))]
    [MemberNotNullWhen(false, nameof(Mensagem))]
    public bool Sucesso { get; private set; }

    public T? Dados { get; private set; }
    public string Mensagem { get; private set; }
    public TipoErro Tipo { get; private set; }

    protected Result(bool sucesso, T? dados, string mensagem, TipoErro tipo = TipoErro.Validacao)
    {
        Sucesso = sucesso;
        Dados = dados;
        Mensagem = mensagem;
        Tipo = tipo;
    }

    public static Result<T> NaoEncontrado(string mensagem)
        => new(false, default, mensagem, TipoErro.NaoEncontrado);

    public static Result<T> Inesperado(string mensagem)
        => new(false, default, mensagem, TipoErro.Inesperado);

    public static Result<T> Conflito(string mensagem)
        => new(false, default, mensagem, TipoErro.Conflito);

    public static Result<T> Ok(T dados, string mensagem = "Operação realizada com sucesso.")
        => new(true, dados, mensagem);

    public static Result<T> SemConteudo(string mensagem = "Operação realizada com sucesso.")
        => new(true, default, mensagem);

    public static Result<T> Falha(string mensagem)
        => new(false, default, mensagem);
}