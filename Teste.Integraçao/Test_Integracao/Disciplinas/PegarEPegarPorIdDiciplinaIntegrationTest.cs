using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class PegarEPegarPorIdDiciplinaIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public PegarEPegarPorIdDiciplinaIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private List<Disciplina> _disciplinasSeed = new();

    [Fact]
    public async Task Deve_Retornar_Lista_De_Disciplinas_Ativas()
    {
        // 1. ARRANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var disciplinasFakes = new List<Disciplina>
    {
        new Disciplina("Matemática", 60),
        new Disciplina("História", 40),
        new Disciplina("Física", 60)
    };

        await contexto.Disciplinas.AddRangeAsync(disciplinasFakes);
        await contexto.SaveChangesAsync();

        // 2. ACT
        var response = await _client.GetAsync("/api/Disciplinas/");

        // 3. ASSERT

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var lista = await response.Content.ReadFromJsonAsync<List<DisciplinaDtoResponse>>();
        lista.Should().NotBeNull();
        lista!.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Deve_Retornar_Disciplina_Por_Id()
    {
        // 1. ARRANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var disciplinaExistente = Data_Factory.DisciplinaFaker.Generate();
        await contexto.Disciplinas.AddAsync(disciplinaExistente);
        await contexto.SaveChangesAsync();

        // 2. ACT
        var response = await _client.GetAsync($"/api/Disciplinas/{disciplinaExistente.Id}");

        // 3. ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var disciplinaDto = await response.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        disciplinaDto.Should().NotBeNull();
        disciplinaDto!.DisciplinaId.Should().Be(disciplinaExistente.Id);
        disciplinaDto.Nome.Should().Be(disciplinaExistente.Nome.Valor);
        disciplinaDto.CargaHoraria.Should().Be(disciplinaExistente.CargaHoraria.Valor);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Para_Id_Desativada()
    {
        // 1. ARRANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var disciplinaExistente = Data_Factory.DisciplinaFaker.Generate();
        disciplinaExistente.Desativar();

        await contexto.Disciplinas.AddAsync(disciplinaExistente);
        await contexto.SaveChangesAsync();

        // 2. ACT
        var response = await _client.GetAsync($"/api/Disciplinas/{disciplinaExistente.Id}");

        // 3. ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Para_Id_Inexistente()
    {
        // ARRANGE
        var idInexistente = Guid.NewGuid();
        // ACT
        var response = await _client.GetAsync($"/api/Disciplinas/{idInexistente}");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_Lista_Vazia_Quando_Nao_Houver_Disciplinas()
    {
        // ARRANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexto.Disciplinas.ExecuteDeleteAsync();
        // ACT
        var response = await _client.GetAsync("/api/Disciplinas");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deve_Retornar_Lista_Vazia_Quando_Nao_Houver_Disciplinas_Ativas()
    {
        // ARRANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var disciplinas = await contexto.Disciplinas.ToListAsync();
        disciplinas.ForEach(d => d.Desativar());
        await contexto.SaveChangesAsync();
        // ACT
        var response = await _client.GetAsync("/api/Disciplinas");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
}