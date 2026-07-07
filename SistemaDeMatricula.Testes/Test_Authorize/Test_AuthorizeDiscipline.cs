using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Authorize;

[Collection("ApiMatrix")]
public class Test_AuthorizeDiscipline : IntegrationTestBase, IAsyncLifetime
{
    private const string RoleAdmin = "Admin";

    private const string RoleEstudante = "Estudante";

    public Test_AuthorizeDiscipline(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", role);
    }

    public DisciplinaDtoCreate CriarDisciplina()
    {
        var dto = DataFactory.DisciplinaFaker.Generate();

        var disciplinaDtoCreate = new DisciplinaDtoCreate(
            Nome: dto.Nome,
            CargaHoraria: dto.CargaHoraria
        );

        return disciplinaDtoCreate;
    }

    [Fact]
    public async Task Test_Authorize_POST_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = CriarDisciplina();
        // Act & Assert
        SetRole(RoleAdmin);
        var responseAdmin = await _client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        Assert.Equal(System.Net.HttpStatusCode.Created, responseAdmin.StatusCode);

        SetRole(RoleEstudante);
        var responseEstudante = await _client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_GET_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = CriarDisciplina();
        SetRole(RoleAdmin);
        var responseAdmin = await _client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // Act & Assert
        SetRole(RoleAdmin);
        var responseGetAdmin = await _client.GetAsync($"/api/disciplinas/{createdDisciplina!.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetAdmin.StatusCode);
        SetRole(RoleEstudante);
        var responseGetEstudante = await _client.GetAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_GETALL_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = CriarDisciplina();
        SetRole(RoleAdmin);
        var responseAdmin = await _client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // Act & Assert
        SetRole(RoleAdmin);
        var responseGetAdmin = await _client.GetAsync($"/api/disciplinas/");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetAdmin.StatusCode);
        SetRole(RoleEstudante);
        var responseGetEstudante = await _client.GetAsync($"/api/disciplinas/");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_DELETE_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = CriarDisciplina();
        SetRole(RoleAdmin);
        var responseAdmin = await _client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // Act & Assert
        SetRole(RoleAdmin);
        var responseGetAdmin = await _client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, responseGetAdmin.StatusCode);
        SetRole(RoleEstudante);
        var responseGetEstudante = await _client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseGetEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_Atualizar_Com_e_sem_Autorizacao()
    {
        var disciplinaDtoCreate = CriarDisciplina();
        SetRole(RoleAdmin);
        var responseAdmin = await _client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        SetRole(RoleAdmin);
        var atulizadoadmin = new DisciplinaDtoUpdate(createdDisciplina.DisciplinaId, "Disciplina Atualizada", 60, true);
        var responseUpdateAdmin = await _client.PutAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}", atulizadoadmin);
        Assert.Equal(System.Net.HttpStatusCode.OK, responseUpdateAdmin.StatusCode);
        SetRole(RoleEstudante);
        var responseUpdateEstudante = await _client.PutAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}", atulizadoadmin);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseUpdateEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_Restaurar_Com_e_sem_Autorizacao()
    {
        // ARRANGE + ACT ADMIN
        var disciplinaDtoCreate = CriarDisciplina();
        SetRole(RoleAdmin);
        var responseAdmin = await _client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        SetRole(RoleAdmin);
        await _client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        var responseRestoreAdmin = await _client.PatchAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}/restaurar", new { });
        Assert.Equal(System.Net.HttpStatusCode.OK, responseRestoreAdmin.StatusCode);

        SetRole(RoleAdmin);
        await _client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");

        SetRole(RoleEstudante);
        var responseRestoreestudante = await _client.PatchAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}/restaurar", new { });

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseRestoreestudante.StatusCode);
    }
}