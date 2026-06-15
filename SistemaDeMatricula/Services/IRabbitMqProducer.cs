namespace SistemaDeMatricula.Services;

public interface IRabbitMqProducer
{
    Task EnviarMensagemAsync<T>(T mensagem, string escola_matricula_exchange);
}