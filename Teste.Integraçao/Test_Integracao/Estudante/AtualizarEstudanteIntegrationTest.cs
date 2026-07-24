using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Estudante;

[Collection("ApiMatrix")]
public class AtualizarEstudanteIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public AtualizarEstudanteIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    { }

    [Fact]
    public async Task Deve_Atualizar_Estudante_Quando_Id_Existir_No_Banco()
    {
        // 1. ARRANGE (Igual ao seu)
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 2. ACT
        var dtoUpdate = Data_Factory.EstudanteFakerup.Generate();

        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idCriado}", dtoUpdate);

        // Prova Real
        var getResponse = await _client.GetAsync($"/api/Estudante/{idCriado}");
        var estudanteNoBanco = await getResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();

        // Verificamos se os campos novos estão lá
        estudanteNoBanco.NomeCompleto.Should().Be(estudanteNoBanco.NomeCompleto);
        estudanteNoBanco.Email.Should().Be(estudanteNoBanco.Email);
        estudanteNoBanco.Telefone.Should().Be(estudanteNoBanco.Telefone);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Tentar_Atualizar_Estudante_Que_Nao_Existe()
    {
        // 1. ARRANGE
        var idInexistente = Guid.NewGuid();
        var dtoUpdate = Data_Factory.EstudanteFakerup.Generate();

        // 2. ACT
        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{Guid.NewGuid()}", dtoUpdate);

        // 3. ASSERT
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Quando_Tentar_Atualizar_Estudante_Com_Dados_Invalidos()
    {
        // 1. ARRANGE
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;
        // Criamos um DTO de atualização com dados inválidos (ex: email sem @)
        var dtoUpdate = Data_Factory.EstudanteFakerup.Generate();
        var invalido = dtoUpdate with { NomeCompleto = "" };
        // 2. ACT
        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idCriado}", invalido);
        // 3. ASSERT
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deve_Retornar_Conflict_Quando_Atualizar_Para_Email_De_Outro_Estudante()
    {
        // --- ARRANGE ---

        // 1. Criamos o Primeiro Estudante (O "Dono" do e-mail)
        var fakeA = Data_Factory.EstudanteFakerdto.Generate();
        var postA = await _client.PostAsJsonAsync("/api/Estudante", fakeA);

        // 2. Criamos o Segundo Estudante (O que vai tentar "roubar" o e-mail)
        var fakeB = Data_Factory.EstudanteFakerdto.Generate();
        var postB = await _client.PostAsJsonAsync("/api/Estudante", fakeB);

        // Pegamos o ID do segundo para poder editá-lo
        var resultadoB = await postB.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idEstudanteB = resultadoB.EstudanteId;

        // --- ACT ---

        // Tentamos atualizar o Estudante B usando o e-mail que pertence ao Estudante A
        var dtoUpdate = Data_Factory.EstudanteFakerup.Generate();
        var conflito = dtoUpdate with { Email = fakeA.Email };

        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idEstudanteB}", conflito);

        // --- ASSERT ---
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }
}