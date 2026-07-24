using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Services;
using SistemaDeMatricula.Testes.Test_Integracao.Setup.Config;
using System.Data.Common;
using Respawn;
using Respawn.Graph;

namespace SistemaDeMatricula.Testes.Test_Integracao.Setup;

public class SistemaMatriculaFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static bool _migrationExecutada = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("JWT_KEY", "ChaveTotalmenteFakeParaOsTestesDeIntegracaoPassaremSemPerigo2026!");

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddEnvironmentVariables();

            // 🎯 O PULO DO GATO SEGURO:
            // Em vez de olhar para a Factory, olha para a classe Program da sua API Principal!
            // Assim, qualquer projeto de teste que rodar vai ler o mesmo cofre de Secrets da API.
            config.AddUserSecrets<Program>(optional: true);

            var settings = config.Build();

            var connectionString = settings["ConnectionStrings:TestConnection"]
                                   ?? settings["ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrEmpty(connectionString))
            {
                // 🔒 AGORA SIM: Deixe vazio e limpo aqui! Sem risco de vazar nada no Git.
                connectionString = "Server=localhost;Port=3306;Database=SistemaMatricula_Testes_DB;Uid=root;Pwd=;";
            }

            config.AddInMemoryCollection(new Dictionary<string, string?>
    {
        { "ConnectionStrings:DefaultConnection", connectionString }
    });
        });
        builder.ConfigureServices(services =>
        {
            var descriptorRabbit = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRabbitMqProducer));

            if (descriptorRabbit != null)
            {
                services.Remove(descriptorRabbit);
            }

            var rabbitMock = new Mock<IRabbitMqProducer>();
            rabbitMock.Setup(x => x.EnviarMensagemAsync(It.IsAny<object>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);

            services.AddSingleton(rabbitMock.Object);

            var descriptorAuth = services.SingleOrDefault(
                d => d.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider));

            if (descriptorAuth != null)
            {
                services.Remove(descriptorAuth);
            }

            var authOptionsDescriptors = services.Where(d =>
                 d.ServiceType.IsGenericType &&
                 (d.ServiceType.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Options.IConfigureOptions<>) ||
                  d.ServiceType.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Options.IPostConfigureOptions<>)) &&
                 d.ServiceType.GetGenericArguments()[0] == typeof(Microsoft.AspNetCore.Authentication.AuthenticationOptions))
                 .ToList();

            foreach (var descriptor in authOptionsDescriptors)
            {
                services.Remove(descriptor);
            }

            // Também removemos os esquemas específicos do JwtBearer que foram registrados nas opções
            var jwtBearerOptionsDescriptors = services.Where(d =>
                d.ServiceType.IsGenericType &&
                d.ServiceType.GetGenericArguments()[0] == typeof(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions))
                .ToList();

            foreach (var descriptor in jwtBearerOptionsDescriptors)
            {
                services.Remove(descriptor);
            }

            // Agora injetamos o esquema de testes do zero, sem rastro do Bearer antigo!
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
        });
    }

    private Respawner? _respawner;
    private DbConnection? _dbConnection;
    private static bool _databaseCreated = false;

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await _semaphore.WaitAsync();
        try
        {
            if (!_migrationExecutada)
            {
                // 🎯 GARANTIA TOTAL DE ESTADO LIMPO:
                // Se a base existir sem a tabela de histórico de migrations (inconsistente),
                // recria o banco limpo para o MigrateAsync rodar perfeito.
                if (await contexto.Database.CanConnectAsync())
                {
                    await contexto.Database.EnsureDeletedAsync();
                }

                await contexto.Database.MigrateAsync();
                _migrationExecutada = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        _dbConnection = new MySqlConnector.MySqlConnection(connectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.MySql,
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection != null && _respawner != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
    }

    public async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
    }
}