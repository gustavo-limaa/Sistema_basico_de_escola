using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Respawn;
using Respawn.Graph;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Services;
using System.Collections.Generic;
using System.Data.Common;

namespace SistemaDeMatricula.Testes.Test_Integracao.Setup;

public class SistemaMatriculaFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("JWT_KEY", "ChaveTotalmenteFakeParaOsTestesDeIntegracaoPassaremSemPerigo2026!");

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddEnvironmentVariables();
            config.AddUserSecrets(typeof(SistemaMatriculaFactory).Assembly, optional: true);

            var settings = config.Build();

            var connectionString = settings["ConnectionStrings:TestConnection"]
                                   ?? settings["ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=localhost;Port=3307;Database=SistemaMatricula_Testes_DB;Uid=root;Pwd=;";
            }

            config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", connectionString }
        });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRabbitMqProducer));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var rabbitMock = new Mock<IRabbitMqProducer>();
            rabbitMock.Setup(x => x.EnviarMensagemAsync(It.IsAny<object>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);

            services.AddSingleton(rabbitMock.Object);
        });
    }

    private Respawner? _respawner;
    private DbConnection? _dbConnection;
    private static bool _databaseCreated = false;

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await contexto.Database.EnsureCreatedAsync();
        // SÓ CRIA O BANCO SE FOR O PRIMEIRO TESTE DE TODOS
        if (!_databaseCreated)
        {
            await contexto.Database.EnsureCreatedAsync();
            _databaseCreated = true;
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