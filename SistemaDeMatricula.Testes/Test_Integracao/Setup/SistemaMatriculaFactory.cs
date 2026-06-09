using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using SistemaDeMatricula.Infraestrutura.Data;
using System.Collections.Generic;
using System.Data.Common;

namespace SistemaDeMatricula.Testes.Test_Integracao.Setup;

public class SistemaMatriculaFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configura o ambiente no local correto
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddEnvironmentVariables();
            config.AddJsonFile("appsettings.json", optional: true);

            var settings = config.Build();
            var connectionString = settings["ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("CONFIGURAÇÃO FALHOU: ConnectionString está vazia!");
            }
        });
    }

    private Respawner? _respawner;
    private DbConnection? _dbConnection;

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await contexto.Database.EnsureCreatedAsync();

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