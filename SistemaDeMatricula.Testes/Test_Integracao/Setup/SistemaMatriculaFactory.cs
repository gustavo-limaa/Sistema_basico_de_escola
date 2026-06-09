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
            // 1. Limpa fontes de configuração para evitar conflito com arquivos locais
            config.Sources.Clear();

            // 2. Adiciona variáveis de ambiente (onde o GitHub injeta a string)
            config.AddEnvironmentVariables();

            // 3. Opcional: Adiciona o json apenas se existir
            config.AddJsonFile("appsettings.json", optional: true);

            var settings = config.Build();

            // 4. Força a leitura da string que injetamos
            var connectionString = settings["ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("CONFIGURAÇÃO FALHOU: ConnectionString está vazia!");
            }
        });
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