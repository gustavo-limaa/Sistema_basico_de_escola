using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class SoftDeleteDisciplinaIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory;

    public SoftDeleteDisciplinaIntegrationTest(SistemaMatriculaFactory factory)
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

    private async Task<DisciplinaDtoResponse> CriarDisciplina()
    {
        var criarResponse = DataFactory.DisciplinaFaker.Generate();
        var dto = new DisciplinaDtoCreate(
            Nome: criarResponse.Nome,
            CargaHoraria: criarResponse.CargaHoraria
        );
        var resultResponse = await _client.PostAsJsonAsync("/api/disciplinas", dto);
        return await resultResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
    }

    [Fact]
    public async Task SoftDeleteDisciplina_Sucesso()
    {
        // 1. Arrange
        var disciplinaCriada = await CriarDisciplina();

        // 2. Act
        var deleteResponse = await _client.DeleteAsync($"/api/disciplinas/{disciplinaCriada.DisciplinaId}");

        // 3. Assert - Parte 1: Status Code
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // 4. Assert - Parte 2: A prova do crime (Verificar no Banco)
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Usamos o IgnoreQueryFilters para conseguir ver a disciplina inativa
        var disciplinaNoBanco = await contexto.Disciplinas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DisciplinaId == disciplinaCriada.DisciplinaId);

        disciplinaNoBanco.Should().NotBeNull();
        disciplinaNoBanco!.Ativo.Should().BeFalse(); // PROVA que foi Soft Delete

        // 5. Assert - Parte 3: Verificar se a API "escondeu" ela no GET
        var getResponse = await _client.GetAsync($"/api/disciplinas/{disciplinaCriada.DisciplinaId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SoftDeleteDisciplina_NaoEncontrada()
    {
        // 1. Arrange
        var idInexistente = Guid.NewGuid();
        // 2. Act
        var deleteResponse = await _client.DeleteAsync($"/api/disciplinas/{idInexistente}");
        // 3. Assert
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SoftDeleteDisciplina_JaInativa()
    {
        // 1. Arrange
        var disciplinaCriada = await CriarDisciplina();
        // Deletar pela primeira vez (Soft Delete)
        var firstDeleteResponse = await _client.DeleteAsync($"/api/disciplinas/{disciplinaCriada.DisciplinaId}");
        firstDeleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        // 2. Act - Tentar deletar novamente a mesma disciplina
        var secondDeleteResponse = await _client.DeleteAsync($"/api/disciplinas/{disciplinaCriada.DisciplinaId}");
        // 3. Assert - A API deve responder que não encontrou a disciplina, pois ela já está inativa
        secondDeleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SoftDeleteDisciplina_EfetivoNoGetTodas()
    {
        // 1. Arrange
        var disciplinaCriada = await CriarDisciplina();
        // Deletar a disciplina criada
        var deleteResponse = await _client.DeleteAsync($"/api/disciplinas/{disciplinaCriada.DisciplinaId}");
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        // 2. Act - Tentar obter todas as disciplinas
        var getResponse = await _client.GetAsync("/api/disciplinas");
        // 3. Assert - A API deve responder com NoContent, pois a única disciplina criada foi inativada
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
}