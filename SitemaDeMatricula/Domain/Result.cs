namespace SitemaDeMatricula.Domain;

public class Result<T>
{
    public bool Sucesso { get; private set; }
    public T? Dados { get; private set; }
    public string Mensagem { get; private set; }

    protected Result(bool sucesso, T? dados, string mensagem)
    {
        Sucesso = sucesso;
        Dados = dados;
        Mensagem = mensagem;
    }

    public static Result<T> Ok(T dados) => new(true, dados, "Operação realizada com sucesso.");

    public static Result<T> Falha(string mensagem) => new(false, default, mensagem);
}