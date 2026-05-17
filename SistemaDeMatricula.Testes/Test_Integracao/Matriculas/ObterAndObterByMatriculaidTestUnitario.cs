using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Test_Integracao.Turmas;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;

namespace SistemaDeMatricula.Testes.Test_Integracao.Matriculas;

[Collection("ApiMatrix")]
public class ObterAndObterByMatriculaidTestUnitario
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public ObterAndObterByMatriculaidTestUnitario(SistemaMatriculaFactory factory)
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
    public async Task Obter_Matricula_por_Id_com_Sucesso()
    {
        // Arrange
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();
        var response = await _client.GetAsync($"/api/matriculas/{matricula.Id}");
        // Act
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request falhou com status {response.StatusCode}. Conteúdo: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var matriculaObtida = await response.Content.ReadFromJsonAsync<MatriculaDtoResponse>();
        // Assert
        Assert.NotNull(matriculaObtida);
        Assert.Equal(matricula.EstudanteId, matriculaObtida.EstudanteId);
        Assert.Equal(matricula.TurmaId, matriculaObtida.TurmaId);
    }

    [Fact]
    public async Task Obter_Matricula_por_Id_Inexistente()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        // Act
        var response = await _client.GetAsync($"/api/matriculas/{idInexistente}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Obter_Matricula_por_Id_com_Id_Inválido()
    {
        // Arrange
        var idInvalido = "123"; // Não é um GUID
        // Act
        var response = await _client.GetAsync($"/api/matriculas/{idInvalido}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Listar_Todas_Matriculas_com_Sucesso()
    {
        // Arrange
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();
        // Act
        var response = await _client.GetAsync("/api/matriculas");
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request falhou com status {response.StatusCode}. Conteúdo: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var matriculasObtidas = await response.Content.ReadFromJsonAsync<IEnumerable<MatriculaDtoResponse>>();
        // Assert
        Assert.NotNull(matriculasObtidas);
        Assert.Contains(matriculasObtidas, m => m.MatriculaId == matricula.Id);
    }

    [Fact]
    public async Task Listar_Todas_Matriculas_Quando_Não_Houver_Nenhuma()
    {
        // Arrange
        // (Não precisamos preparar dados, pois queremos testar o cenário sem matrículas)
        // Act
        var response = await _client.GetAsync("/api/matriculas");
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request falhou com status {response.StatusCode}. Conteúdo: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var matriculasObtidas = await response.Content.ReadFromJsonAsync<IEnumerable<MatriculaDtoResponse>>();
        // Assert
        Assert.NotNull(matriculasObtidas);
        Assert.Empty(matriculasObtidas);
    }
}