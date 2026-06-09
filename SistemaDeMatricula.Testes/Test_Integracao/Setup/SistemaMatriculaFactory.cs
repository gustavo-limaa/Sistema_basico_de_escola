using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Infraestrutura.Data;
using System.Collections.Generic;

namespace SistemaDeMatricula.Testes.Test_Integracao.Setup;

public class SistemaMatriculaFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Adiciona suporte a variáveis de ambiente (essencial para o GitHub Actions)
            config.AddEnvironmentVariables();

            var settings = config.Build();

            // Tenta buscar "TestConnection" ou cai na "DefaultConnection"
            var connectionString = settings.GetConnectionString("TestConnection")
                                   ?? settings.GetConnectionString("DefaultConnection");

            // Se, mesmo assim, for nulo, significa que algo correu mal no pipeline
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("String de conexão não encontrada! Verifique o ambiente.");
            }

            config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", connectionString }
        }
            );
        }
        );
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await contexto.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // APAGA TUDO SÓ QUANDO TODOS OS TESTES TERMINAREM
        await contexto.Database.EnsureDeletedAsync();
    }
}