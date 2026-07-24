using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Integracao.Matriculas;

[Collection("ApiMatrix")]
public class DesativarMatriculaIntegrationTest : IntegrationTestBase
{
    public DesativarMatriculaIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(EstudanteEntity estudante, TurmaEntity turma, MatriculaEntity matricula)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await Data_Factory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Desativar_Matricula_com_Sucesso()
    {
        // 1. Arrange
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

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