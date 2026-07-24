using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class CriarDiciplinaIntegracaoTest : IntegrationTestBase, IAsyncLifetime
{
    public CriarDiciplinaIntegracaoTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Criar_Disciplina_Quando_Dados_Validos()
    {
        // 1. ARRANGE
        var dtoParaEnviar = Data_Factory.DisciplinaFakerdto.Generate();

        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoParaEnviar);

        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        resultadoPost.Should().NotBeNull();
        resultadoPost!.Nome.Should().Be(dtoParaEnviar.Nome);
        resultadoPost.CargaHoraria.Should().Be(dtoParaEnviar.CargaHoraria);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Nome_Vazio()
    {
        var dtoValido = Data_Factory.DisciplinaFakerdto.Generate();
        var dtoInvalido = dtoValido with { Nome = string.Empty };

        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoInvalido);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Nome_Existente()
    {
        // 1. ARRANGE
        var dtoValido = Data_Factory.DisciplinaFakerdto.Generate();

        var postResponse1 = await _client.PostAsJsonAsync("/api/Disciplinas", dtoValido);
        postResponse1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        // 2. ACT
        var postResponse2 = await _client.PostAsJsonAsync("/api/Disciplinas", dtoValido);
        // 3. ASSERT
        postResponse2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_CargaHoraria_Excessiva()
    {
        // 1. ARRANGE
        var dto = Data_Factory.DisciplinaFakerdto.Generate();
        var invalidacao = dto with { CargaHoraria = 10000 };
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", invalidacao);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Nome_Muito_Curto()
    {
        var dtoValido = Data_Factory.DisciplinaFakerdto.Generate();
        var dtoInvalido = dtoValido with { Nome = "AB" }; // curto de verdade

        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dtoInvalido);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_Nome_Muito_Longo()
    {
        // 1. ARRANGE
        var nomeMuitoLongo = new string('A', 101); // Nome com 101 caracteres é inválido
        var dtoInvalido = Data_Factory.DisciplinaFakerdto.Generate();
        var dto = dtoInvalido with { Nome = nomeMuitoLongo };
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dto);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Quando_CargaHoraria_Zero()
    {
        // 1. ARRANGE
        var dtoInvalido = Data_Factory.DisciplinaFakerdto.Generate();
        var dto = dtoInvalido with { CargaHoraria = 0 };
        // 2. ACT
        var postResponse = await _client.PostAsJsonAsync("/api/Disciplinas", dto);
        // 3. ASSERT
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}