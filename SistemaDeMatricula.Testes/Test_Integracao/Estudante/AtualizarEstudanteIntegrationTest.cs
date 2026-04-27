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
public class AtualizarEstudanteIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory;

    public AtualizarEstudanteIntegrationTest(SistemaMatriculaFactory factory)
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
    public async Task Deve_Atualizar_Estudante_Quando_Id_Existir_No_Banco()
    {
        // 1. ARRANGE (Igual ao seu)
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 2. ACT
        var dataNascimentoAtualizada = estudanteFake.DataNascimento.Valor.AddYears(-1);
        var dtoUpdate = new EstudanteDtoUpdate(
            "Nome Atualizado",
            "email.atualizado@exemplo.com",
            dataNascimentoAtualizada,
            "(11) 99999-9999"
        );

        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idCriado}", dtoUpdate);

        // 3. ASSERT (O Upgrade)
        putResponse.IsSuccessStatusCode.Should().BeTrue("a API deveria permitir a atualização");

        // Prova Real: Faz um GET para ver se o banco realmente mudou
        var getResponse = await _client.GetAsync($"/api/Estudante/{idCriado}");
        var estudanteNoBanco = await getResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();

        // Verificamos se os campos novos estão lá
        estudanteNoBanco.NomeCompleto.Should().Be("Nome Atualizado");
        estudanteNoBanco.Email.Should().Be("email.atualizado@exemplo.com");
        estudanteNoBanco.Telefone.Should().Be("11999999999");
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Tentar_Atualizar_Estudante_Que_Nao_Existe()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();

        var dataNascimentoAtualizada = estudanteFake.DataNascimento.Valor.AddYears(-1);
        // 1. ARRANGE
        var idInexistente = Guid.NewGuid();
        var dtoUpdate = new EstudanteDtoUpdate(
            "Nome Atualizado",
            "email.atualizado@exemplo.com",
            dataNascimentoAtualizada,
            "11999999999"
        );

        // 2. ACT
        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idInexistente}", dtoUpdate);

        // 3. ASSERT
        putResponse.IsSuccessStatusCode.Should().BeFalse("a API deveria retornar Not Found");
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound, "o status deveria ser 404 Not Found");
    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Quando_Tentar_Atualizar_Estudante_Com_Dados_Invalidos()
    {
        // 1. ARRANGE
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;
        // Criamos um DTO de atualização com dados inválidos (ex: email sem @)
        var dataNascimentoAtualizada = estudanteFake.DataNascimento.Valor.AddYears(-1);
        var dtoUpdateInvalido = new EstudanteDtoUpdate(
            "Nome Válido",
            "email.invalido.com", // Email sem @
            dataNascimentoAtualizada,
            "11999999999"
        );
        // 2. ACT
        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idCriado}", dtoUpdateInvalido);
        // 3. ASSERT
        putResponse.IsSuccessStatusCode.Should().BeFalse("a API deveria validar os dados e retornar Bad Request");
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest, "o status deveria ser 400 Bad Request");
    }

    [Fact]
    public async Task Deve_Retornar_Conflict_Quando_Atualizar_Para_Email_De_Outro_Estudante()
    {
        // --- ARRANGE ---

        // 1. Criamos o Primeiro Estudante (O "Dono" do e-mail)
        var fakeA = DataFactory.EstudanteFaker.Generate();
        var postA = await _client.PostAsJsonAsync("/api/Estudante",
            new EstudanteDtoCreate(fakeA.NomeCompleto.Valor, fakeA.Email.Valor, fakeA.DataNascimento.Valor, fakeA.Cpf.Valor, fakeA.Telefone.Valor));

        // 2. Criamos o Segundo Estudante (O que vai tentar "roubar" o e-mail)
        var fakeB = DataFactory.EstudanteFaker.Generate();
        var postB = await _client.PostAsJsonAsync("/api/Estudante",
            new EstudanteDtoCreate(fakeB.NomeCompleto.Valor, fakeB.Email.Valor, fakeB.DataNascimento.Valor, fakeB.Cpf.Valor, fakeB.Telefone.Valor));

        // Pegamos o ID do segundo para poder editá-lo
        var resultadoB = await postB.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idEstudanteB = resultadoB.EstudanteId;

        // --- ACT ---

        // Tentamos atualizar o Estudante B usando o e-mail que pertence ao Estudante A
        var dtoUpdateConflito = new EstudanteDtoUpdate(
            "Nome do B",
            fakeA.Email.Valor, // <--- Aqui está o crime! Usando o e-mail do A
            fakeB.DataNascimento.Valor,
            "11999999999"
        );

        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idEstudanteB}", dtoUpdateConflito);

        // --- ASSERT ---
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict, "porque o e-mail já pertence a outra pessoa");
    }
}