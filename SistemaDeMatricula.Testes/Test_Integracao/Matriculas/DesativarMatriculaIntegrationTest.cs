using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Integracao.Matriculas;

[Collection("ApiMatrix")]
public class DesativarMatriculaIntegrationTest
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public DesativarMatriculaIntegrationTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // 1. ANTES DE CADA TESTE: Não precisamos de nada especial aqui
    public Task InitializeAsync() => Task.CompletedTask;

    // 2. DEPOIS DE CADA TESTE: Aqui é onde a mágica da limpeza acontece
    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Apaga quem depende de todo mundo (A última ponta)
        await contexto.Matriculas.ExecuteDeleteAsync();

        // 2. Apaga as Turmas (que dependem de Professor e Disciplina)
        await contexto.Turmas.ExecuteDeleteAsync();

        // 3. Agora o banco deixa apagar as raízes
        await contexto.Estudantes.ExecuteDeleteAsync();
        await contexto.Professores.ExecuteDeleteAsync();
        await contexto.Disciplinas.ExecuteDeleteAsync();
    }

    private async Task<(EstudanteEntity estudante, TurmaEntity turma, MatriculaEntity matricula)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await DataFactory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Desativar_Matricula_com_Sucesso()
    {
        // Arrange
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        // Act
        var response = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var resultadoReal = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(resultadoReal);
    }

    [Fact]
    public async Task Desativar_Matricula_Inexistente()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/matriculas/{idInexistente}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Desativar_Matricula_JaDesativada()
    {
        // Arrange
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();
        // Primeiro, desativamos a matrícula
        var primeiraResposta = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");
        Assert.Equal(System.Net.HttpStatusCode.OK, primeiraResposta.StatusCode);
        // Act - Tentamos desativar novamente
        var segundaResposta = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, segundaResposta.StatusCode);
    }

    [Fact]
    public async Task Desativar_Matricula_IdInvalido()
    {
        // Arrange
        var idInvalido = Guid.Empty;
        // Act
        var response = await _client.DeleteAsync($"/api/matriculas/{idInvalido}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Desativar_Matricula_ErroNoBanco()
    {
        // Arrange
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();
        // Simulando um erro no banco de dados (ex: bloqueando a tabela de matrículas)
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexto.Database.ExecuteSqlRawAsync("ALTER TABLE Matriculas NOCHECK CONSTRAINT ALL");
        try
        {
            // Act
            var response = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }
        finally
        {
            // Restaurando a tabela para não afetar outros testes
            await contexto.Database.ExecuteSqlRawAsync("ALTER TABLE Matriculas CHECK CONSTRAINT ALL");
        }
    }
}