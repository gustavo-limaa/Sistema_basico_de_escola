using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Authorize;

public class Test_AuthorizeDiscipline : PermissaoTestBase
{
    public Test_AuthorizeDiscipline(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Test_Authorize_POST_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = Data_Factory.DisciplinaFakerdto.Generate();
        // Act & Assert
        ResetarParaAdmin();
        var responseAdmin = await _Client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        Assert.Equal(System.Net.HttpStatusCode.Created, responseAdmin.StatusCode);

        AutenticarComoEstudante();
        var responseEstudante = await _Client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_GET_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = Data_Factory.DisciplinaFakerdto.Generate();
        ResetarParaAdmin();
        var responseAdmin = await _Client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // Act & Assert

        var responseGetAdmin = await _Client.GetAsync($"/api/disciplinas/{createdDisciplina!.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetAdmin.StatusCode);

        AutenticarComoEstudante();
        var responseGetEstudante = await _Client.GetAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_GETALL_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = Data_Factory.DisciplinaFakerdto.Generate();

        ResetarParaAdmin();

        var responseAdmin = await _Client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // Act & Assert
        var responseGetAdmin = await _Client.GetAsync($"/api/disciplinas/");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetAdmin.StatusCode);

        AutenticarComoEstudante();
        var responseGetEstudante = await _Client.GetAsync($"/api/disciplinas/");
        Assert.Equal(System.Net.HttpStatusCode.OK, responseGetEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_DELETE_Com_e_sem_Autorizacao()
    {
        // Arrange
        var disciplinaDtoCreate = Data_Factory.DisciplinaFakerdto.Generate();

        ResetarParaAdmin();
        var responseAdmin = await _Client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // Act & Assert

        var responseGetAdmin = await _Client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, responseGetAdmin.StatusCode);
        AutenticarComoEstudante();
        var responseGetEstudante = await _Client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseGetEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_Atualizar_Com_e_sem_Autorizacao()
    {
        var disciplinaDtoCreate = Data_Factory.DisciplinaFakerdto.Generate();
        ResetarParaAdmin();
        var responseAdmin = await _Client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        var atulizadoadmin = Data_Factory.DisciplinaFakerdto.Generate();
        var responseUpdateAdmin = await _Client.PutAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}", atulizadoadmin);
        Assert.Equal(System.Net.HttpStatusCode.OK, responseUpdateAdmin.StatusCode);
        AutenticarComoEstudante();
        var responseUpdateEstudante = await _Client.PutAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}", atulizadoadmin);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseUpdateEstudante.StatusCode);
    }

    [Fact]
    public async Task Test_Authorize_Restaurar_Com_e_sem_Autorizacao()
    {
        // ARRANGE + ACT ADMIN
        var disciplinaDtoCreate = Data_Factory.DisciplinaFakerdto.Generate();
        ResetarParaAdmin();

        var responseAdmin = await _Client.PostAsJsonAsync("/api/disciplinas", disciplinaDtoCreate);
        var createdDisciplina = await responseAdmin.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        await _Client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");
        var responseRestoreAdmin = await _Client.PatchAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}/restaurar", new { });
        Assert.Equal(System.Net.HttpStatusCode.OK, responseRestoreAdmin.StatusCode);

        await _Client.DeleteAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}");

        AutenticarComoEstudante();
        var responseRestoreestudante = await _Client.PatchAsJsonAsync($"/api/disciplinas/{createdDisciplina.DisciplinaId}/restaurar", new { });

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, responseRestoreestudante.StatusCode);
    }
}