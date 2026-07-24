using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class SoftDeleteProfessorIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public SoftDeleteProfessorIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<Guid> CadastrarProfessorERetornarIdAsync()
    {
        var dto = Data_Factory.ProfessorFakerdto.Generate();
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        var criado = await response.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        return criado!.ProfessorId;
    }

    // 2. Auxiliar de Assert: O "Raio-X" que ignora o filtro global para ver se o dado ainda existe
    private async Task<bool> VerificarSeProfessorEstaInativoNoBanco(Guid id)
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // IgnoreQueryFilters é a chave para ver os "fantasmas"
        var professor = await contexto.Professores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        // Retorna true apenas se ele existir E o campo Ativo for false
        return professor != null && !professor.Ativo;
    }

    [Fact]
    public async Task Deletar_Professor_Deve_Mudar_Status_Para_Inativo_No_Banco()
    {
        // 1. Arrange: Usa o auxiliar para já ter um ID válido no banco
        var idParaDeletar = await CadastrarProfessorERetornarIdAsync();

        // 2. Act: Executa o Delete
        var responseDelete = await _client.DeleteAsync($"/api/professores/{idParaDeletar}");

        // 3. Assert
        responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent); // 204

        // 4. Verificação de Superfície: O GET normal não deve achar ele (404)
        var responseGet = await _client.GetAsync($"/api/professores/{idParaDeletar}");
        responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 5. Verificação de Subsolo: Usa o Raio-X para confirmar que ele virou "Inativo"
        var estaInativo = await VerificarSeProfessorEstaInativoNoBanco(idParaDeletar);
        estaInativo.Should().BeTrue();
    }

    [Fact]
    public async Task Deletar_Professor_Inexistente_Deve_Retornar_NotFound()
    {
        // Arrange: Geramos um ID aleatório que não existe no banco
        var idInexistente = Guid.NewGuid();
        // Act: Tentamos deletar esse ID
        var responseDelete = await _client.DeleteAsync($"/api/professores/{idInexistente}");
        // Assert: Esperamos um 404 Not Found
        responseDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deletar_Professor_Ja_Inativo_Deve_Retornar_NotFound()
    {
        // Arrange
        var id = await CadastrarProfessorERetornarIdAsync();
        await _client.DeleteAsync($"/api/professores/{id}");
        // Act
        var responseDeleteNovamente = await _client.DeleteAsync($"/api/professores/{id}");
        // Assert
        responseDeleteNovamente.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deletar_Professor_Ja_Inativo_Deve_Retornar_NotFound_2()
    {
        // Arrange
        var id = await CadastrarProfessorERetornarIdAsync();
        await _client.DeleteAsync($"/api/professores/{id}");
        // Act
        var responseDeleteNovamente = await _client.DeleteAsync($"/api/professores/{id}");
        // Assert
        responseDeleteNovamente.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deletar_Professor_Com_Id_Invalido_Deve_Retornar_BadRequest()
    {
        // Arrange
        var idInvalido = "12345";
        // Act
        var responseDelete = await _client.DeleteAsync($"/api/professores/{idInvalido}");
        // Assert
        responseDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deletar_Professor_E_Verificar_Se_Ele_Nao_Aparece_Mais_Em_Lista_De_Todos()
    {
        // Arrange
        var id = await CadastrarProfessorERetornarIdAsync();
        await _client.DeleteAsync($"/api/professores/{id}");
        // Act
        var responseGetTodos = await _client.GetAsync("/api/professores");
        var listaProfessores = await responseGetTodos.Content.ReadFromJsonAsync<List<ProfessorDtoResponse>>();
        // Assert
        listaProfessores.Should().NotContain(p => p.ProfessorId == id);
    }

    [Fact]
    public async Task Deletar_Professor_Deve_Sumir_Da_Api_Mas_Continuar_Inativo_No_Banco()
    {
        var id = await CadastrarProfessorERetornarIdAsync();

        // 2. Act
        var responseDelete = await _client.DeleteAsync($"/api/professores/{id}");

        var responseGet = await _client.GetAsync($"/api/professores/{id}");
        responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var professorNoBanco = await contexto.Professores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        professorNoBanco.Should().NotBeNull();
        professorNoBanco!.Ativo.Should().BeFalse();
    }
}