using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class PegarEPegarPorIdDiciplinaIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory;

    public PegarEPegarPorIdDiciplinaIntegrationTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private List<Disciplina> _disciplinasSeed = new();

    public async Task InitializeAsync()
    {
        // Criamos um escopo para acessar o banco antes do teste começar
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Geramos 3 disciplinas fakes
        _disciplinasSeed = DataFactory.DisciplinaFaker.Generate(3);

        // 2. Salvamos no banco
        await contexto.Disciplinas.AddRangeAsync(_disciplinasSeed);
        await contexto.SaveChangesAsync();
    }

    // A faxina depois de cada GET
    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Limpa a tabela para o próximo teste de GET entrar no banco vazio
        await contexto.Disciplinas.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Deve_Retornar_Lista_De_Disciplinas_Ativas()
    {
        // ACT
        var response = await _client.GetAsync("/api/Disciplina");

        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<DisciplinaDtoResponse>>();

        lista.Should().NotBeNull();
        lista!.Count.Should().BeGreaterThanOrEqualTo(3); // Garante que as 3 que criamos estão lá
    }

    [Fact]
    public async Task Deve_Retornar_Disciplina_Por_Id()
    {
        // ARRANGE
        var disciplinaExistente = _disciplinasSeed.First();
        // ACT
        var response = await _client.GetAsync($"/api/Disciplina/{disciplinaExistente.DisciplinaId}");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var disciplinaDto = await response.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaDto.Should().NotBeNull();
        disciplinaDto!.DisciplinaId.Should().Be(disciplinaExistente.DisciplinaId);
        disciplinaDto.Nome.Should().Be(disciplinaExistente.Nome.Valor);
        disciplinaDto.CargaHoraria.Should().Be(disciplinaExistente.CargaHoraria.Valor);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Para_Id_Inexistente()
    {
        // ARRANGE
        var idInexistente = Guid.NewGuid();
        // ACT
        var response = await _client.GetAsync($"/api/Disciplina/{idInexistente}");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Para_Id_Desativada()
    {
        // ARRANGE
        var disciplinaExistente = _disciplinasSeed.First();
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var disciplinaNoBanco = await contexto.Disciplinas.FindAsync(disciplinaExistente.DisciplinaId);
        disciplinaNoBanco!.Desativar();
        await contexto.SaveChangesAsync();
        // ACT
        var response = await _client.GetAsync($"/api/Disciplina/{disciplinaExistente.DisciplinaId}");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_Lista_Vazia_Quando_Nao_Houver_Disciplinas()
    {
        // ARRANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexto.Disciplinas.ExecuteDeleteAsync(); // Limpa a tabela
        // ACT
        var response = await _client.GetAsync("/api/Disciplina");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deve_Retornar_Lista_Vazia_Quando_Nao_Houver_Disciplinas_Ativas()
    {
        // ARRANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Desativa todas as disciplinas
        var disciplinas = await contexto.Disciplinas.ToListAsync();
        disciplinas.ForEach(d => d.Desativar());
        await contexto.SaveChangesAsync();
        // ACT
        var response = await _client.GetAsync("/api/Disciplina");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
}