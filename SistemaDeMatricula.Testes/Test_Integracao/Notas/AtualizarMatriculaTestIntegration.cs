using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Integracao.Notas;

[Collection("ApiMatrix")]
public class AtualizarMatriculaTestIntegration
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public AtualizarMatriculaTestIntegration(SistemaMatriculaFactory factory)
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

    private NotaDtoUpdate GerarNotaAtualizada(double valor = 8.0) => new NotaDtoUpdate(
        Valor: valor,
        Descricao: "Desempenho bom, mas pode melhorar",
        Importancia: TipoImportancia.Media,
        Categoria: CategoriaAvaliacao.Prova
    );

    [Fact]
    public async Task Atualizar_com_Sucesso()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();

        var notaAtualizada = GerarNotaAtualizada();
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada.Id}", notaAtualizada);
        resposta.EnsureSuccessStatusCode();

        var notaObtida = await resposta.Content.ReadFromJsonAsync<NotaDtoResponse>();

        Assert.NotNull(notaObtida);
        Assert.Equal(notaCriada.Id, notaObtida.Id);
    }

    [Fact]
    public async Task Atualizar_Nota_NaoEncontrada()
    {
        var (matricula, _) = await PrepararCenarioDeNota();
        var notaAtualizada = GerarNotaAtualizada();
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{Guid.NewGuid()}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Nota_MatriculaIncorreta()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();
        var notaAtualizada = GerarNotaAtualizada();
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{Guid.NewGuid()}/notas/{notaCriada.Id}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Nota_ValorInvalido()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();
        var notaAtualizada = GerarNotaAtualizada(valor: 15.0); // Valor inválido
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada.Id}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Nota_ValorNegativo()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();
        var notaAtualizada = GerarNotaAtualizada(valor: -5.0); // Valor inválido
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada.Id}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}