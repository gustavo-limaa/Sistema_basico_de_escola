using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        // 1. Apague as notas primeiro (elas dependem de Matricula)
        await contexto.Database.ExecuteSqlRawAsync("DELETE FROM notas");

        // 2. Apague as Matriculas (dependem de Estudante e Turma)
        await contexto.Matriculas.ExecuteDeleteAsync();

        // 3. Apague as Turmas (dependem de Professor e Disciplina)
        await contexto.Turmas.ExecuteDeleteAsync();

        // 4. Agora as raízes
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
        // 1. Arrange
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        // 2. Verificação de Pré-condição (Aqui o Copilot tinha razão, verifique ANTES)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var matAtiva = await db.Matriculas.FindAsync(matricula.Id);
            Assert.True(matAtiva.Ativo, "A matrícula deveria estar ativa ANTES de desativar!");
        }

        // Act
        var response = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var erroContent = await response.Content.ReadAsStringAsync();
            // Isso vai imprimir no painel de saída do teste exatamente o que o Result retornou
            throw new Exception($"Esperava OK, mas recebi BadRequest. Erro: {erroContent}");
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // 5. Verificação de Pós-condição (Verifique se mudou DEPOIS)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var matInativa = await db.Matriculas.FindAsync(matricula.Id);
            Assert.False(matInativa.Ativo, "A matrícula deveria estar inativa APÓS desativar!");
        }
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
}