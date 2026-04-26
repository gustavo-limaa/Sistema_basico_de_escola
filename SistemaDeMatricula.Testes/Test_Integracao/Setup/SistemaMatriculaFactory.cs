using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SitemaDeMatricula.InfraEstrutura.Data;
using System.Collections.Generic;

namespace SistemaDeMatricula.Testes.Testes_Integracao.Setup;

public class SistemaMatriculaFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // 1. O .NET monta as configurações (lê appsettings, segredos, etc.)
            var settings = config.Build();

            // 2. Buscamos a string de conexão que você criou no secrets.json
            var connectionString = settings.GetConnectionString("TestConnection");

            // 3. "Enganamos" a API: dizemos que a DefaultConnection dela
            // agora é o valor que pegamos do nosso banco de testes.
            config.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "ConnectionStrings:DefaultConnection", connectionString! }
        });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // CRIA O BANCO UMA ÚNICA VEZ PARA TODA A BATERIA DE TESTES
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