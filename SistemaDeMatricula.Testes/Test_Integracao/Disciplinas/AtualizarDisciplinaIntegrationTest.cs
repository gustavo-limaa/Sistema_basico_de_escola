using Bogus.DataSets;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class AtualizarDisciplinaIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory;

    public AtualizarDisciplinaIntegrationTest(SistemaMatriculaFactory factory)
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
    public async Task AtualizarDisciplina_Sucesso()
    {
        // 1. Criar uma disciplina para atualizar
        var criarResponse = DataFactory.DisciplinaFaker.Generate();

        var dto = new DisciplinaDtoCreate(

            Nome: criarResponse.Nome,
            CargaHoraria: criarResponse.CargaHoraria

        );

        var resultResponse = await _client.PostAsJsonAsync("/api/disciplina", dto);

        var disciplinaCriada = await resultResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // 2. Atualizar a disciplina criada
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplina/{disciplinaCriada!.DisciplinaId}", new DisciplinaDtoUpdate(
            DisciplinaId: disciplinaCriada.DisciplinaId,
            Nome: "Matemática Avançada",
            CargaHoraria: 80,
            Ativo: true));

        if (!atualizarResponse.IsSuccessStatusCode)
        {
            var mensagemErro = await atualizarResponse.Content.ReadAsStringAsync();
            throw new Exception($"A API retornou erro {atualizarResponse.StatusCode}: {mensagemErro}");
        }

        var disciplinaAtualizada = await atualizarResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaAtualizada.Should().NotBeNull();
        disciplinaAtualizada.Nome.Should().Be("Matemática Avançada");
        disciplinaAtualizada.CargaHoraria.Should().Be(80);
        disciplinaAtualizada.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task AtualizarDisciplina_NaoEncontrada()
    {
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplina/{Guid.NewGuid()}", new DisciplinaDtoUpdate(
            DisciplinaId: Guid.NewGuid(),
            Nome: "Matemática Avançada",
            CargaHoraria: 80,
            Ativo: true));
        atualizarResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AtualizarDisciplina_NomeDuplicado()
    {
        // Criar a primeira disciplina
        var criarResponse1 = DataFactory.DisciplinaFaker.Generate();
        var dto1 = new DisciplinaDtoCreate(
            Nome: criarResponse1.Nome,
            CargaHoraria: criarResponse1.CargaHoraria
        );
        var resultResponse1 = await _client.PostAsJsonAsync("/api/disciplina", dto1);
        resultResponse1.EnsureSuccessStatusCode();
        var disciplinaCriada1 = await resultResponse1.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // Criar a segunda disciplina
        var criarResponse2 = DataFactory.DisciplinaFaker.Generate();
        var dto2 = new DisciplinaDtoCreate(
            Nome: criarResponse2.Nome,
            CargaHoraria: criarResponse2.CargaHoraria
        );
        var resultResponse2 = await _client.PostAsJsonAsync("/api/disciplina", dto2);
        resultResponse2.EnsureSuccessStatusCode();
        var disciplinaCriada2 = await resultResponse2.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // Tentar atualizar a segunda disciplina com o nome da primeira
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplina/{disciplinaCriada2!.DisciplinaId}", new DisciplinaDtoUpdate(
            DisciplinaId: disciplinaCriada2.DisciplinaId,
            Nome: disciplinaCriada1!.Nome, // Nome duplicado
            CargaHoraria: 80,
            Ativo: true));
        atualizarResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarDisciplina_DadosInvalidos()
    {
        // Criar uma disciplina para atualizar
        var criarResponse = DataFactory.DisciplinaFaker.Generate();
        var dto = new DisciplinaDtoCreate(
            Nome: criarResponse.Nome,
            CargaHoraria: criarResponse.CargaHoraria
        );
        var resultResponse = await _client.PostAsJsonAsync("/api/disciplina", dto);
        resultResponse.EnsureSuccessStatusCode();
        var disciplinaCriada = await resultResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // Tentar atualizar com dados inválidos (nome vazio e carga horária negativa)
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplina/{disciplinaCriada!.DisciplinaId}", new DisciplinaDtoUpdate(
            DisciplinaId: disciplinaCriada.DisciplinaId,
            Nome: "", // Nome inválido
            CargaHoraria: -10, // Carga horária inválida
            Ativo: true));
        atualizarResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarDisciplina_SemAlteracoes()
    {
        // Criar uma disciplina para atualizar
        var criarResponse = DataFactory.DisciplinaFaker.Generate();
        var dto = new DisciplinaDtoCreate(
            Nome: criarResponse.Nome,
            CargaHoraria: criarResponse.CargaHoraria
        );
        var resultResponse = await _client.PostAsJsonAsync("/api/disciplina", dto);
        resultResponse.EnsureSuccessStatusCode();
        var disciplinaCriada = await resultResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // Tentar atualizar sem alterar os dados (mesmo nome e carga horária)
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplina/{disciplinaCriada!.DisciplinaId}", new DisciplinaDtoUpdate(
            DisciplinaId: disciplinaCriada.DisciplinaId,
            Nome: disciplinaCriada.Nome, // Mesmo nome
            CargaHoraria: disciplinaCriada.CargaHoraria, // Mesma carga horária
            Ativo: disciplinaCriada.Ativo)); // Mesmo status
        atualizarResponse.EnsureSuccessStatusCode();
        var disciplinaAtualizada = await atualizarResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaAtualizada.Should().NotBeNull();
        disciplinaAtualizada.Nome.Should().Be(disciplinaCriada.Nome);
        disciplinaAtualizada.CargaHoraria.Should().Be(disciplinaCriada.CargaHoraria);
        disciplinaAtualizada.Ativo.Should().Be(disciplinaCriada.Ativo);
    }
}