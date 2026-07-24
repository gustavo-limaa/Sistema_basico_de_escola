using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class PegarTodosEPegarPorIdProfessorIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public PegarTodosEPegarPorIdProfessorIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Pegar_Todos_Professores_Retorna_Lista_Com_Professores_Ativos()
    {
        // 1. ARRANGE: O banco está limpo pelo Respawn, então nós criamos os 7 professores aqui
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Gerar 7 professores usando Bogus ou criando manualmente
        var professoresFakes = Data_Factory
            .ProfessorFaker
            .Generate(7);

        await contexto.Professores.AddRangeAsync(professoresFakes);
        await contexto.SaveChangesAsync();

        // 2. ACT: Agora sim, chamamos o GET
        var response = await _client.GetAsync("/api/professores");

        // 3. ASSERT: Agora o banco tem os 7 que criamos para ESTE teste
        var professores = await response.Content.ReadFromJsonAsync<List<ProfessorDtoResponse>>();
        professores.Count.Should().Be(7);
    }

    [Fact]
    public async Task Pegar_Professor_Por_Id_Retorna_Professor_Se_Existe_E_Ativo()
    {
        // Arrange
        var dto = Data_Factory.ProfessorFakerdto.Generate();
        var response1 = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultado = await response1.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        var response2 = await _client.GetAsync($"/api/professores/{resultado.ProfessorId}");
        var professorEncontrado = await response2.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Assert
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        professorEncontrado.Should().NotBeNull();
    }

    [Fact]
    public async Task Pegar_Professor_Por_Id_Retorna_NotFound_Se_Professor_Nao_Existir()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        // Act
        var response = await _client.GetAsync($"/api/professores/{idInexistente}");
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Pegar_Professor_Por_Id_Retorna_BadRequest_Se_Id_For_Invalido()
    {
        // Arrange
        var idInvalido = "123";
        // Act
        var response = await _client.GetAsync($"/api/professores/{idInvalido}");
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Restaurar_Professor_Desativado_Deve_Voltar_A_Ser_Exibido_No_Get()
    {
        // 1. Arrange
        var dto = Data_Factory.ProfessorFakerdto.Generate();
        var responsePost = await _client.PostAsJsonAsync("/api/professores", dto);
        var criado = await responsePost.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        await _client.DeleteAsync($"/api/professores/{criado.ProfessorId}");

        var responseGetInativo = await _client.GetAsync($"/api/professores/{criado.ProfessorId}");
        responseGetInativo.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound
            );

        // 2. Act
        var responseRestore = await _client.PatchAsync($"/api/professores/{criado.ProfessorId}/restaurar", null);

        if (responseRestore.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            var erroDetalhado = await responseRestore.Content.ReadAsStringAsync();
            throw new Exception($"ERRO 500 DETALHADO: {erroDetalhado}");
        }

        // 3. Assert
        var responseGetAtivo = await _client.GetAsync($"/api/professores/{criado.ProfessorId}");
        responseGetAtivo.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var restaurado = await responseGetAtivo.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        restaurado.ProfessorId.Should().Be(criado.ProfessorId);
        restaurado.NomeCompleto.Should().Be(dto.NomeCompleto);
    }
}