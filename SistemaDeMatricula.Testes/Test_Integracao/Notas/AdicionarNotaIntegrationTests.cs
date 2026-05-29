using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Integracao.Notas;

[Collection("ApiMatrix")]
public class AdicionarNotaIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public AdicionarNotaIntegrationTests(SistemaMatriculaFactory factory)
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

    // Dentro da classe de teste ou de uma classe auxiliar (ex: TestDataBuilder)
    private async Task<(Matricula matricula, NotaDtoCreate novaNota)> PrepararCenarioDeNota(double valor = 9.5)
    {
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        var novaNota = new NotaDtoCreate(
            Valor: valor,
            Descricao: "Excelente desempenho",
            Importancia: TipoImportancia.Alta,
            Categoria: CategoriaAvaliacao.FeiraDeCiencias
        );

        return (matricula, novaNota);
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await DataFactory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Sucesso()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(9.5);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);

        // Assert
        response.EnsureSuccessStatusCode();
        var notaCriada = await response.Content.ReadFromJsonAsync<NotaDtoResponse>();

        Assert.NotNull(notaCriada);
        Assert.Equal(novaNota.Valor, notaCriada.Valor);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Valor_Negativo_Deve_Falhar()
    {
        // Arrange
        var (matricula, _) = await PrepararCenarioDeNota();
        var notaNegativa = new NotaDtoCreate(-1.0, "Errado", TipoImportancia.Alta, CategoriaAvaliacao.FeiraDeCiencias);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaNegativa);
        var responseContent = await response.Content.ReadAsStringAsync();

        // DEBUG: Imprima o conteúdo real que o teste está vendo
        System.Diagnostics.Debug.WriteLine($"JSON RECEBIDO: {responseContent}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Teste menos rigoroso para ver se o conteúdo é o que esperamos
        Assert.True(responseContent.Contains("Nota"), $"Esperava que a mensagem contivesse 'Nota', mas veio: {responseContent}");
    }

    [Fact]
    public async Task Adiciona_Notas_para_Matricula_Inexistente_Deve_Falhar()
    {
        // Arrange
        var novaNota = new NotaDtoCreate(
            Valor: 8.0,
            Descricao: "Boa participação",
            Importancia: TipoImportancia.Media
            ,
            Categoria: CategoriaAvaliacao.Prova
        );
        var matriculaIdInexistente = Guid.NewGuid(); // ID aleatório que não existe
        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matriculaIdInexistente}/notas", novaNota);
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Adiciona_Notas_para_Matricula_Inativa_Deve_Falhar()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(7.0);
        // Inativa a matrícula
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var matriculaDb = await contexto.Matriculas.FirstOrDefaultAsync(m => m.Id == matricula.Id);
        if (matriculaDb != null)
        {
            matriculaDb.desativar();
            await contexto.SaveChangesAsync();
        }
        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não é possível adicionar notas a uma matrícula inativa", errorMessage);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Valor_Zero_Deve_Ser_Sucesso()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(0.0);
        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        // Assert
        response.EnsureSuccessStatusCode();
        var notaCriada = await response.Content.ReadFromJsonAsync<NotaDtoResponse>();
        Assert.NotNull(notaCriada);
        Assert.Equal(0.0, notaCriada.Valor);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Valor_Muito_Alto_Deve_Ser_Sucesso()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(100.0);
        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        // Assert
        response.EnsureSuccessStatusCode();
        var notaCriada = await response.Content.ReadFromJsonAsync<NotaDtoResponse>();
        Assert.NotNull(notaCriada);
        Assert.Equal(100.0, notaCriada.Valor);
    }

    [Theory]
    [InlineData(9.5)]
    [InlineData(0.0)]
    [InlineData(100.0)]
    public async Task Adiciona_Notas_com_Sucesso_Variados(double valor)
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(valor);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);

        // Assert
        response.EnsureSuccessStatusCode();
        var notaCriada = await response.Content.ReadFromJsonAsync<NotaDtoResponse>();
        Assert.Equal(valor, notaCriada.Valor);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Valor_Negativo_Deve_Falhar_Variados()
    {
        // Arrange
        var (matricula, _) = await PrepararCenarioDeNota();
        var valoresNegativos = new[] { -0.01, -1.0, -100.0 };
        foreach (var valor in valoresNegativos)
        {
            var notaNegativa = new NotaDtoCreate(valor, "Valor negativo", TipoImportancia.Alta, CategoriaAvaliacao.FeiraDeCiencias);
            // Act
            var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaNegativa);
            var responseContent = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Nota", responseContent);
        }
    }
}