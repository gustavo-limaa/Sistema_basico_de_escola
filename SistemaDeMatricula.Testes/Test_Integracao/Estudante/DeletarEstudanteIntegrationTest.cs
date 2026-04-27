using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Estudante;

[Collection("ApiMatrix")]
public class DeletarEstudanteIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public DeletarEstudanteIntegrationTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // 1. ANTES DE CADA TESTE: Não precisamos de nada especial aqui
    public Task InitializeAsync() => Task.CompletedTask;

    // 2. DEPOIS DE CADA TESTE: Aqui é onde a mágica da limpeza acontece
    public async Task DisposeAsync()
    {
        // Criamos um "escopo" para conseguir pegar o AppDbContext lá de dentro da API
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Agora sim! O 'contexto' existe aqui e podemos limpar a tabela
        await contexto.Estudantes.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Deve_Deletar_Estudante_Quando_Id_Existir_No_Banco()
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
        // 2. ACT: Agora sim, tentamos deletar pelo ID que acabou de ser gerado
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idCriado}");
        // 3. ASSERT
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        // Verifica se realmente foi deletado tentando buscar de novo
        var getResponse = await _client.GetAsync($"/api/Estudante/{idCriado}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Id_Nao_Existir_No_Banco()
    {
        // 1. ARRANGE: Gera um ID aleatório que não existe
        var idInexistente = Guid.NewGuid();
        // 2. ACT: Tenta deletar usando esse ID
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idInexistente}");
        // 3. ASSERT
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Quando_Id_For_Invalido()
    {
        // 1. ARRANGE: Cria um ID que não é um GUID válido
        var idInvalido = "12345";
        // 2. ACT: Tenta deletar usando esse ID inválido
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idInvalido}");
        // 3. ASSERT
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Id_For_Empty_Guid()
    {
        var idVazio = Guid.Empty; // 00000000-0000...
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idVazio}");

        // De acordo com seu código, você retorna NotFound aqui
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Quando_Id_For_Invalido_texto()
    {
        var idInvalido = "texto-qualquer-nao-guid";
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idInvalido}");

        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}