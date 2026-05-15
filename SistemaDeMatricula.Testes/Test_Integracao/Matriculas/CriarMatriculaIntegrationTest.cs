using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

// No seu método, você usa o apelido:

namespace SistemaDeMatricula.Testes.Test_Integracao.Matriculas;

[Collection("ApiMatrix")]
public class CriarMatriculaIntegrationTest : IAsyncLifetime

{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public CriarMatriculaIntegrationTest(SistemaMatriculaFactory factory)
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

    private async Task<(EstudanteEntity, TurmaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await DataFactory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Criar_Matricula_com_Sucesso()
    {
        var (_, turma) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado

        // Cria um NOVO estudante que ainda não está na turma
        var novoEstudante = DataFactory.EstudanteFaker.Generate();

        // Salva ele no banco (precisa do scope aqui se não moveu para o construtor)
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexto.Estudantes.AddAsync(novoEstudante);
        await contexto.SaveChangesAsync();

        var dto = new MatriculaDtoCreate(novoEstudante.Id, turma.Id);
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK); // Ou 201 Created
    }

    [Fact]
    public async Task Criar_Matricula_com_falha()
    {
        // Bem mais limpo!
        var (estudante, turma) = await PrepararDadosNoBanco();

        var dto = new MatriculaDtoCreate(Guid.NewGuid(), turma.Id);
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Matricula_com_falha_por_misclick()
    {
        // Bem mais limpo!
        var (estudante, turma) = await PrepararDadosNoBanco();

        var dto = new MatriculaDtoCreate(estudante.Id, turma.Id);
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        var post2 = await _client.PostAsJsonAsync("/api/matriculas", postResponse);

        post2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Turma_Estiver_Lotada()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Criamos um cenário onde a turma só tem 1 vaga
        var (alun, turma) = await DataFactory.CriarCenarioDeMatriculaValido(contexto, capacidade: 1);

        // CONFERÊNCIA:
        var contagemNoBanco = await contexto.Matriculas.CountAsync(m => m.TurmaId == turma.Id && m.Ativo);
        // Se isso aqui for 0, o erro está no DataFactory (não salvou ou salvou inativo)
        contagemNoBanco.Should().Be(1);
        // Aluno 2 tenta entrar na mesma turma
        var aluno2 = DataFactory.EstudanteFaker.Generate();
        await contexto.Estudantes.AddAsync(aluno2);
        await contexto.SaveChangesAsync();

        var dto = new MatriculaDtoCreate(aluno2.Id, turma.Id);
        var response = await _client.PostAsJsonAsync("/api/matriculas", dto);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        // Opcional: verificar se a mensagem é "Turma lotada"
    }
}