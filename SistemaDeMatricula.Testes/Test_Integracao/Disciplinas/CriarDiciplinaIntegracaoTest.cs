using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class CriarDiciplinaIntegracaoTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory;

    public CriarDiciplinaIntegracaoTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // O que fazer antes de cada teste de GET
    public Task InitializeAsync() => Task.CompletedTask;

    // A faxina depois de cada GET
    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Limpa a tabela para o próximo teste de GET entrar no banco vazio
        await contexto.Disciplinas.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Deve_Criar_Disciplina_Quando_Dados_Validos()
    {
        // 1. ARRANGE
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        var dtoParaEnviar = new DisciplinaDtoCreate(
            disciplinaFake.Nome.Valor,
            disciplinaFake.CargaHoraria.Valor
        );

        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoParaEnviar);

        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        resultadoPost.Should().NotBeNull();
        resultadoPost!.Nome.Should().Be(dtoParaEnviar.Nome);
        resultadoPost.CargaHoraria.Should().Be(dtoParaEnviar.CargaHoraria);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Dados_Invalidos()
    {
        // 1. ARRANGE

        var dtoInvalido = new DisciplinaDtoCreate(
            Nome: "", // Nome vazio é inválido
            CargaHoraria: -5 // Carga horária negativa é inválida
        );
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoInvalido);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Nome_Existente()
    {
        // 1. ARRANGE
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        var dtoValido = new DisciplinaDtoCreate(
            disciplinaFake.Nome.Valor,
            disciplinaFake.CargaHoraria
        );
        // Criamos a disciplina pela API para garantir que o nome já exista no banco
        var postResponse1 = await _client.PostAsJsonAsync("/api/Disciplinas", dtoValido);
        postResponse1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        // 2. ACT - Tentamos criar outra disciplina com o mesmo nome
        var postResponse2 = await _client.PostAsJsonAsync("/api/Disciplinas", dtoValido);
        // 3. ASSERT - Esperamos um BadRequest por causa do nome duplicado
        postResponse2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_CargaHoraria_Excessiva()
    {
        // 1. ARRANGE
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        var dtoInvalido = new DisciplinaDtoCreate(
            disciplinaFake.Nome.Valor,
            CargaHoraria: 1000 // Carga horária excessiva é inválida
        );
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoInvalido);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Nome_Muito_Curto()
    {
        // 1. ARRANGE
        var dtoInvalido = new DisciplinaDtoCreate(
            Nome: "A", // Nome muito curto é inválido
            CargaHoraria: 40
        );
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoInvalido);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Nome_Muito_Longo()
    {
        // 1. ARRANGE
        var nomeMuitoLongo = new string('A', 101); // Nome com 101 caracteres é inválido
        var dtoInvalido = new DisciplinaDtoCreate(
            Nome: nomeMuitoLongo,
            CargaHoraria: 40
        );
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoInvalido);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_CargaHoraria_Zero()
    {
        // 1. ARRANGE
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        var dtoInvalido = new DisciplinaDtoCreate(
            disciplinaFake.Nome.Valor,
            CargaHoraria: 0 // Carga horária zero é inválida
        );
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoInvalido);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}