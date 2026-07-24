using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;
using System.Text;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class RestaurarDisciplinaIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public RestaurarDisciplinaIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<DisciplinaDtoResponse> CriarDisciplina()
    {
        var dto = Data_Factory.DisciplinaFakerdto.Generate();
        var resultResponse = await _client.PostAsJsonAsync("/api/disciplinas", dto);
        return await resultResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
    }

    [Fact]
    public async Task Criar_Disciplina_DesativaDisciplina_RetornaDisciplinaRestaurada()
    {
        // Arrange
        var disciplina = await CriarDisciplina();

        var deleteResponse = await _client.DeleteAsync($"/api/disciplinas/{disciplina.DisciplinaId}");
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent); // 👈 garante que desativou de verdade

        // Act
        var response = await _client.PatchAsync($"/api/disciplinas/{disciplina.DisciplinaId}/restaurar", new StringContent("", Encoding.UTF8, "application/json"));
        var resultado = await response.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        resultado.Should().NotBeNull();
        resultado!.DisciplinaId.Should().Be(disciplina.DisciplinaId);
        resultado.Ativo.Should().BeTrue();

        var getResponse = await _client.GetAsync($"/api/disciplinas/{disciplina.DisciplinaId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var disciplinaFinal = await getResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaFinal!.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Restaurar_Disciplina_Com_Id_Inexistente_Retorna_NotFound()
    {
        // Act
        var response = await _client.PatchAsync($"/api/disciplinas/{Guid.NewGuid()}/restaurar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Restaurar_Disciplina_Ativa_RetornaConflict()
    {
        // Arrange
        var disciplina = await CriarDisciplina();
        // Act
        var response = await _client.PatchAsJsonAsync($"/api/disciplinas/{disciplina.DisciplinaId}/restaurar", new StringContent("", Encoding.UTF8, "application/json"));
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }
}