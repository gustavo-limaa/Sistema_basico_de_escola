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

    /// <summary>
    /// Sucesso com dados e mensagem opcional
    /// </summary>
    /// <param name="dados"></param>
    /// <param name="mensagem"></param>
    /// <returns></returns>
    public static Result<T> Ok(T dados, string mensagem = "Operação realizada com sucesso.")
        => new(true, dados, mensagem);

    /// <summary>
    /// Sucesso sem dados (para Updates/Deletes)
    /// usar para deletar ou atualizar sem retornar dados, apenas a mensagem de sucesso
    /// </summary>
    /// <param name="mensagem"></param>
    /// <returns></returns>
    public static Result<T> SemConteudo(string mensagem = "Operação realizada com sucesso.")
        => new(true, default, mensagem);

    public static Result<T> Falha(string mensagem)
        => new(false, default, mensagem);
}