using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Estudante;

[Collection("ApiMatrix")] // <--- Não esqueça de entrar na mesma "Matrix"
public class GetsEstudanteIntegrationTest : IntegrationTestBase, IAsyncLifetime // <--- O segredo da limpeza
{
    public GetsEstudanteIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Retornar_Estudante_Quando_Id_Existir_No_Banco()
    {
        // 1. ARRANGE
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 2. ACT
        var getResponse = await _client.GetAsync($"/api/Estudante/{idCriado}");

        // 3. ASSERT
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var estudanteRetornado = await getResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        estudanteRetornado?.EstudanteId.Should().Be(idCriado);
        estudanteRetornado?.NomeCompleto.Should().Be(dtoCreate.NomeCompleto);
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Id_Nao_Existir_No_Banco()
    {
        // 1. ARRANGE
        var idInexistente = Guid.NewGuid();
        // 2. ACT
        var getResponse = await _client.GetAsync($"/api/Estudante/{idInexistente}");
        // 3. ASSERT
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_Todos_Estudantes_Quando_Fizer_Get_Sem_Id()
    {
        var dtoCreate1 = Data_Factory.EstudanteFakerdto.Generate();

        await _client.PostAsJsonAsync("/api/Estudante", dtoCreate1);
        var dtoCreate2 = Data_Factory.EstudanteFakerdto.Generate();
        await _client.PostAsJsonAsync("/api/Estudante", dtoCreate2);
        // 2. ACT
        var getResponse = await _client.GetAsync("/api/Estudante");
        // 3. ASSERT
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var estudantesRetornados = await getResponse.Content.ReadFromJsonAsync<List<EstudanteDtoResponse>>();
        // 1. Primeiro garante que a lista não é nula (evita o erro de "Object Reference")
        estudantesRetornados.Should().NotBeNull("porque a API deve retornar uma lista, mesmo que vazia");

        // 2. Usa o método específico para coleções
        estudantesRetornados.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Deve_Retornar_OK_Quando_Fizer_Get_Sem_Id_E_Nao_Houver_Estudantes()
    {
        // 2. ACT
        var getResponse = await _client.GetAsync("/api/Estudante");
        // 3. ASSERT
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var estudantesRetornados = await getResponse.Content.ReadFromJsonAsync<List<EstudanteDtoResponse>>();
        estudantesRetornados.Should().BeEmpty();
    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Quando_Fizer_Get_Com_Id_Invalido()
    {
        // 1. ARRANGE
        var idInvalido = "12345";
        // 2. ACT
        var getResponse = await _client.GetAsync($"/api/Estudante/{idInvalido}");
        // 3. ASSERT
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}