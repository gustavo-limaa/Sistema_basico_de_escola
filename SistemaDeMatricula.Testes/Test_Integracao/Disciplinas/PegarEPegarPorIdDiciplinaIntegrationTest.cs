using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
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
        // 1. ARRANGE: Inserir as disciplinas no banco limpo
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Supondo que você tenha uma fixture ou crie manualmente as entidades
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
        // Corrigido para OK (200), já que agora injetamos dados!
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var lista = await response.Content.ReadFromJsonAsync<List<DisciplinaDtoResponse>>();
        lista.Should().NotBeNull();
        lista!.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Deve_Retornar_Disciplina_Por_Id()
    {
        // 1. ARRANGE: Criar e salvar uma disciplina real no banco para este teste
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var disciplinaExistente = new Disciplina("Química", 80);
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
        // 1. ARRANGE: Criar, salvar e desativar a disciplina no banco real
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var disciplinaExistente = new Disciplina("Biologia", 40);
        disciplinaExistente.Desativar(); // Certifique-se de que ela já salve inativa se for o caso

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
        await contexto.Disciplinas.ExecuteDeleteAsync(); // Limpa a tabela
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
        // Desativa todas as disciplinas
        var disciplinas = await contexto.Disciplinas.ToListAsync();
        disciplinas.ForEach(d => d.Desativar());
        await contexto.SaveChangesAsync();
        // ACT
        var response = await _client.GetAsync("/api/Disciplinas");
        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
}