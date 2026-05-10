using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class RestaurarDisciplinaIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory;

    public RestaurarDisciplinaIntegrationTest(SistemaMatriculaFactory factory)
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
    public async Task Criar_Disciplina_DesativaDisciplina_RetornaDisciplinaRestaurada()
    {
        // Arrange
        var disciplina = await CriarDisciplina();
        await _client.DeleteAsync($"/api/disciplinas/{disciplina.DisciplinaId}");

        // Act
        var response = await _client.PatchAsync($"/api/disciplinas/{disciplina.DisciplinaId}/restaurar", new StringContent("", Encoding.UTF8, "application/json"));
        var resultado = await response.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK); // 1. Garante o sucesso da rota
        resultado.Should().NotBeNull();
        resultado!.DisciplinaId.Should().Be(disciplina.DisciplinaId);
        resultado.Ativo.Should().BeTrue(); // 2. Prova que o estado mudou

        // 3. A prova real: O sistema voltou a "enxergar" a disciplina?
        var getResponse = await _client.GetAsync($"/api/disciplinas/{disciplina.DisciplinaId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var disciplinaFinal = await getResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaFinal!.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Restaurar_Disciplina_Com_Id_Inexistente_Retorna_NotFound()
    {
        // Act - Tentando restaurar um GUID aleatório que nunca foi criado
        var response = await _client.PatchAsync($"/api/disciplinas/{Guid.NewGuid()}/restaurar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Restaurar_Disciplina_Nao_Existe_RetornaNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        // Act
        var response = await _client.PatchAsync($"/api/disciplinas/{id}/restaurar", new StringContent("", Encoding.UTF8, "application/json"));
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Restaurar_Disciplina_Ativa_RetornaConflict()
    {
        // Arrange
        var disciplina = await CriarDisciplina();
        // Act
        var response = await _client.PatchAsJsonAsync($"/api/disciplinas/{disciplina.DisciplinaId}/restaurar", new StringContent("", Encoding.UTF8, "application/json"));
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }
}