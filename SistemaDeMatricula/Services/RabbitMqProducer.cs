using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace SistemaDeMatricula.Services;

public class RabbitMqProducer : IRabbitMqProducer
{
    private readonly ConnectionFactory _factory;

    public RabbitMqProducer(IConfiguration configuration)
    {
        _factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMqHost"] ?? "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
    }

    public async Task EnviarMensagemAsync<T>(T mensagem, string escola_matricula_exchange)
    {
        using var connection = await _factory.CreateConnectionAsync();

        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: escola_matricula_exchange,
            type: ExchangeType.Fanout,
            durable: true
        );

        var json = JsonSerializer.Serialize(mensagem);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: escola_matricula_exchange,
            routingKey: string.Empty,
            body: body
        );
    }
}