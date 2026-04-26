using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SistemaDeMatricula.Testes.Testes_Integracao.Setup;
using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Estudante;

[Collection("ApiMatrix")] // <--- Não esqueça de entrar na mesma "Matrix"
public class GetsEstudanteIntegrationTest : IAsyncLifetime // <--- O segredo da limpeza
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory;

    public GetsEstudanteIntegrationTest(SistemaMatriculaFactory factory)
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
        await contexto.Estudantes.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Deve_Retornar_Estudante_Quando_Id_Existir_No_Banco()
    {
        // 1. ARRANGE: Cria um estudante real via API primeiro
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        // Faz o POST para garantir que tem alguém no banco
        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 2. ACT: Agora sim, tentamos buscar pelo ID que acabou de ser gerado
        var getResponse = await _client.GetAsync($"/api/Estudante/{idCriado}");

        // 3. ASSERT
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var estudanteRetornado = await getResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        estudanteRetornado.EstudanteId.Should().Be(idCriado);
        estudanteRetornado.NomeCompleto.Should().Be(dtoCreate.NomeCompleto);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Id_Nao_Existir_No_Banco()
    {
        // 1. ARRANGE: Gera um ID aleatório que não existe
        var idInexistente = Guid.NewGuid();
        // 2. ACT: Tenta buscar esse ID inexistente
        var getResponse = await _client.GetAsync($"/api/Estudante/{idInexistente}");
        // 3. ASSERT: Esperamos um NotFound (404)
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_Todos_Estudantes_Quando_Fizer_Get_Sem_Id()
    {
        // 1. ARRANGE: Cria dois estudantes reais via API primeiro
        var estudanteFake1 = DataFactory.EstudanteFaker.Generate();
        var dtoCreate1 = new EstudanteDtoCreate(
            estudanteFake1.NomeCompleto.Valor,
            estudanteFake1.Email.Valor,
            estudanteFake1.DataNascimento.Valor,
            estudanteFake1.Cpf.Valor,
            estudanteFake1.Telefone.Valor
        );
        await _client.PostAsJsonAsync("/api/Estudante", dtoCreate1);
        var estudanteFake2 = DataFactory.EstudanteFaker.Generate();
        var dtoCreate2 = new EstudanteDtoCreate(
            estudanteFake2.NomeCompleto.Valor,
            estudanteFake2.Email.Valor,
            estudanteFake2.DataNascimento.Valor,
            estudanteFake2.Cpf.Valor,
            estudanteFake2.Telefone.Valor
        );
        await _client.PostAsJsonAsync("/api/Estudante", dtoCreate2);
        // 2. ACT: Tenta buscar todos os estudantes
        var getResponse = await _client.GetAsync("/api/Estudante");
        // 3. ASSERT: Esperamos um OK e pelo menos 2 estudantes na lista
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var estudantesRetornados = await getResponse.Content.ReadFromJsonAsync<List<EstudanteDtoResponse>>();
        // 1. Primeiro garante que a lista não é nula (evita o erro de "Object Reference")
        estudantesRetornados.Should().NotBeNull("porque a API deve retornar uma lista, mesmo que vazia");

        // 2. Usa o método específico para coleções
        estudantesRetornados.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Fizer_Get_Sem_Id_E_Nao_Houver_Estudantes()
    {
        // 1. ARRANGE: Garantimos que o banco está vazio (a limpeza do DisposeAsync já faz isso)
        // 2. ACT: Tenta buscar todos os estudantes
        var getResponse = await _client.GetAsync("/api/Estudante");
        // 3. ASSERT: Esperamos um NotFound (404) ou uma lista vazia, dependendo da implementação da API
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var estudantesRetornados = await getResponse.Content.ReadFromJsonAsync<List<EstudanteDtoResponse>>();
        estudantesRetornados.Should().BeEmpty();
    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Quando_Fizer_Get_Com_Id_Invalido()
    {
        // 1. ARRANGE: Define um ID inválido (não é um GUID)
        var idInvalido = "12345";
        // 2. ACT: Tenta buscar usando o ID inválido
        var getResponse = await _client.GetAsync($"/api/Estudante/{idInvalido}");
        // 3. ASSERT: Esperamos um BadRequest (400) por causa do formato do ID
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deve_Retornar_Lista_Vazia_Quando_Nao_Houver_Estudantes() // Nome corrigido
    {
        // 1. ARRANGE: Banco limpo pelo DisposeAsync

        // 2. ACT
        var getResponse = await _client.GetAsync("/api/Estudante");

        // 3. ASSERT
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var estudantesRetornados = await getResponse.Content.ReadFromJsonAsync<List<EstudanteDtoResponse>>();
        estudantesRetornados.Should().BeEmpty();
    }
}