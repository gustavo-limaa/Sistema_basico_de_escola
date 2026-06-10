using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient _client;
    protected readonly SistemaMatriculaFactory _factory;

    protected IntegrationTestBase(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Aqui dentro tu podes usar o Respawn (ou o teu código de delete atual)
        // Se quiseres manter o teu código atual, basta escrever ele aqui uma vez.
        await _factory.ResetDatabaseAsync();
    }
}