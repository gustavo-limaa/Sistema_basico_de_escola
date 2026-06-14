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

            // 1. Tenta ler as variáveis de ambiente (Útil para o GitHub Actions)
            config.AddEnvironmentVariables();

            // 2. Faz a Factory enxergar os User Secrets do seu projeto de TESTES
            // O typeof(SistemaMatriculaFactory).Assembly diz para o .NET buscar os segredos vinculados a este projeto de testes
            config.AddUserSecrets(typeof(SistemaMatriculaFactory).Assembly, optional: true);

            var settings = config.Build();

            // 3. Tenta pegar do ambiente ou do secrets.json local (procurando primeiro pela chave de teste)
            var connectionString = settings["ConnectionStrings:TestConnection"]
                                   ?? settings["ConnectionStrings:DefaultConnection"];

            // 4. Se mesmo assim não achar nada (ex: fallback de segurança), deixamos uma string genérica sem credenciais
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=localhost;Port=3307;Database=SistemaMatricula_Testes_DB;Uid=root;Pwd=;";
            }

            // 5. Sobrescrevemos a configuração em memória com a string definitiva para a API usar
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