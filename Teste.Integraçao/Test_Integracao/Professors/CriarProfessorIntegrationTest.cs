using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class CriarProfessorIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public CriarProfessorIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Criar_Professor_Retorna_Professor_Criado()
    {
        // Arrange

        var dto = Data_Factory.ProfessorFakerdto.Generate();
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultado = await response.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task Criar_Professor_Com_Cpf_Existente_Retorna_Conflict()
    {
        // Arrange
        var dto = Data_Factory.ProfessorFakerdto.Generate();
        // Primeiro criamos o professor normalmente
        var response1 = await _client.PostAsJsonAsync("/api/professores", dto);
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        // Agora tentamos criar outro com o mesmo CPF
        var response2 = await _client.PostAsJsonAsync("/api/professores", dto);
        // Assert
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Criar_Professor_Com_Email_Existente_Retorna_Conflict()
    {
        // Arrange[
        var dto = Data_Factory.ProfessorFakerdto.Generate();
        // Primeiro criamos o professor normalmente
        var response1 = await _client.PostAsJsonAsync("/api/professores", dto);
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        // Agora tentamos criar outro com o mesmo Email
        var professor2 = Data_Factory.ProfessorFakerdto.Generate();
        var conflto = professor2 with { Email = dto.Email };

        var response2 = await _client.PostAsJsonAsync("/api/professores", conflto);
        // Assert
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Criar_Professor_Com_Dados_Invalidos_Retorna_BadRequest()
    {
        // Arrange
        var dto = Data_Factory.ProfessorFakerdto.Generate();
        var conflito = dto with { Cpf = "12314515123412541" };
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", conflito);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Salario_Negativo_Retorna_BadRequest()
    {
        // Arrange
        var dto = Data_Factory.ProfessorFakerdto.Generate();
        var conflito = dto with { Salario = -5000 };
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", conflito);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Categoria_Invalida_Retorna_BadRequest()
    {
        // Arrange

        var dto = Data_Factory.ProfessorFakerdto.Generate();
        var conflito = dto with { Categoria = "" };
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", conflito);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Data_Nascimento_Futura_Retorna_BadRequest()
    {
        // Arrange

        var dto = Data_Factory.ProfessorFakerdto.Generate(); var conflito = dto with { DataNascimento = DateOnly.FromDateTime(DateTime.Now.AddYears(1)) };
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", conflito);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Nome_Vazio_Retorna_BadRequest()
    {
        // Arrange
        var dto = Data_Factory.ProfessorFakerdto.Generate(); var conflito = dto with { NomeCompleto = "" };// Act
        var response = await _client.PostAsJsonAsync("/api/professores", conflito);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}