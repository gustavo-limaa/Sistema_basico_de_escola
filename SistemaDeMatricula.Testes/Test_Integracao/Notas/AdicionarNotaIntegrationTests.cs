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

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await DataFactory.CriarCenarioDeMatriculaValido(contexto);
    }
}