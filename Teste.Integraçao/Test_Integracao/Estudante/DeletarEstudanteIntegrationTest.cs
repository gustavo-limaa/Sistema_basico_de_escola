using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Estudante;

[Collection("ApiMatrix")]
public class DeletarEstudanteIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public DeletarEstudanteIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Deletar_Estudante_Quando_Id_Existir_No_Banco()
    {
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;
        // 2. ACT
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idCriado}");
        // 3. ASSERT
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        var getResponse = await _client.GetAsync($"/api/Estudante/{idCriado}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Id_Nao_Existir_No_Banco()
    {
        // 1. ARRANGE
        var idInexistente = Guid.NewGuid();
        // 2. ACT
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idInexistente}");
        // 3. ASSERT
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}