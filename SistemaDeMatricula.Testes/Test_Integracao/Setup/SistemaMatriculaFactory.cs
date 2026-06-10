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
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();

            // 1. Tenta ler as variáveis de ambiente (Crucial para o GitHub Actions)
            config.AddEnvironmentVariables();

            var settings = config.Build();
            // Pegamos o que veio do ambiente (GitHub usa 'ConnectionStrings:DefaultConnection')
            var connectionString = settings["ConnectionStrings:DefaultConnection"];

            // 2. Se não houver variável de ambiente (significa que você está no seu PC local)
            if (string.IsNullOrEmpty(connectionString))
            {
                // Forçamos a string local perfeita com o banco correto
                connectionString = "Server=localhost;Port=3306;Database=SistemaMatricula_DB;Uid=root;Pwd=158575Z;";
            }

            // 3. Sobrescrevemos a configuração em memória com a string definitiva
            config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", connectionString }
        });
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