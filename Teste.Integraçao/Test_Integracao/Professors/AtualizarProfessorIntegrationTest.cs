using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class AtualizarProfessorIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public AtualizarProfessorIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private ProfessorDtoUpdate CriarDtoUpdate(Guid idExistente, string? emailSobrescrito = null)
    {
        var dto = Data_Factory.ProfessorFakerup.Generate();

        return dto with
        {
            ProfessorId = idExistente,
            Email = emailSobrescrito ?? dto.Email
        };
    }

    [Fact]
    public async Task Ciclo_Completo_Criar_e_Atualizar()
    {
        // 1. Cria um DTO de post e envia
        var dtoPost = Data_Factory.ProfessorFakerdto.Generate();
        var responsePost = await _client.PostAsJsonAsync("/api/professores", dtoPost);
        var criado = await responsePost.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Interligação: Gera um DTO de Update usando o ID do que acabou de ser criado
        var dtoUpdate = CriarDtoUpdate(criado.ProfessorId);

        // 3. Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);

        // 4. Assert
        responsePut.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Atualizar_Professor_Inexistente_Retorna_NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var dtoUpdate = CriarDtoUpdate(idInexistente);
        // Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);
        // Assert
        responsePut.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_Professor_Com_Email_De_Outro_Retorna_Conflict()
    {
        // 1. Arrange: Cria dois professores VÁLIDOS e DIFERENTES
        var response1 = await _client.PostAsJsonAsync("/api/professores", Data_Factory.ProfessorFakerdto.Generate());
        var profA = await response1.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var response2 = await _client.PostAsJsonAsync("/api/professores", Data_Factory.ProfessorFakerdto.Generate());
        var profB = await response2.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Tenta atualizar o Professor B usando o E-mail do Professor A
        var dtoUpdate = CriarDtoUpdate(profB.ProfessorId, emailSobrescrito: profA.Email);

        // 3. Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);

        // 4. Assert
        responsePut.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Atualizar_Professor_Com_Id_Invalido_Retorna_BadRequest()
    {
        // Arrange: Enviamos um objeto anônimo com uma string que não é GUID
        var dtoInvalido = new
        {
            ProfessorId = "nao-sou-um-guid",
            NomeCompleto = "Teste",
            DataNascimento = "1999-01-01",
            Email = "test@example.com",
            Telefone = "11999999999",
            Salario = 5000.00,
            Categoria = "Titular"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/professores", dtoInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Atualizar_Professor_Mantendo_Mesmo_Email_Deve_Retornar_Ok()
    {
        // 1. Arrange:
        var responsePost = await _client.PostAsJsonAsync("/api/professores", Data_Factory.ProfessorFakerdto.Generate());
        var professorCriado = await responsePost.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var dtoUpdate = CriarDtoUpdate(professorCriado.ProfessorId, professorCriado.Email);

        // Garantimos que os outros dados são diferentes (Nome, Telefone, etc virão novos do Bogus)
        // O CPF nem entra aqui, como a gente já sabe!

        // 3. Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);
        var professorAtualizado = await responsePut.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 4. Assert
        responsePut.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Verificamos se o e-mail continuou o mesmo
        professorAtualizado.Email.Should().Be(professorCriado.Email);

        // Verificamos se o nome mudou (provando que a atualização ocorreu)
        professorAtualizado.NomeCompleto.Should().Be(dtoUpdate.NomeCompleto);
        professorAtualizado.NomeCompleto.Should().NotBe(professorCriado.NomeCompleto);
    }
}