using Azure;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Authorize;

[Collection("ApiMatrix")]
public class Test_AuthorizeProfessor : IntegrationTestBase, IAsyncLifetime
{
    public Test_AuthorizeProfessor(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    public ProfessorDtoCreate Criarprrofessor()
    {
        var professor = DataFactory.ProfessorFaker.Generate();

        var dto = new ProfessorDtoCreate
        (
         NomeCompleto: professor.NomeCompleto.Valor,
         Cpf: professor.Cpf.Valor,
         DataNascimento: professor.DataNascimento.Valor,
         Email: professor.Email.Valor,
         Telefone: professor.Telefone.Valor,
         Salario: professor.Salario.Valor,
         Categoria: professor.Categoria.ToString()
        );

        return dto;
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_EndpointProtegido_Retorna200()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        // Act
        var post = await _client.PostAsJsonAsync("/api/professores", dto);
        var postResult = await post.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_EndpointProtegido_deve_falhar()
    {
        // Arrange
        var permisao = "professorr";
        var dto = Criarprrofessor();
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        // Act
        var post = await _client.PostAsJsonAsync("/api/professores", dto);
        var postResult = await post.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Forbidden, post.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_obtertodos_e_deve_falhar()
    {
        // Arrange
        var permisao = "Estudante";

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);
        // Act
        var post = await _client.GetAsync("/api/professores");
        var postResult = await post.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Forbidden, post.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_obtertodos_e_deve_ser_sucesso()
    {
        // Arrange
        var permisao = "Professor";

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);
        // Act
        var post = await _client.GetAsync("/api/professores");
        var postResult = await post.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_obterporid_e_deve_ser_sucesso()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var dto = Criarprrofessor();

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var permisao = "Professor";
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var getByIdResponse = await _client.GetAsync($"/api/professores/{resultadoPost.ProfessorId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_obterporid_e_deve_ser_falha()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante");

        var getByIdResponse = await _client.GetAsync($"/api/professores/{resultadoPost.ProfessorId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, getByIdResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_obterporCPF_e_deve_ser_falha()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante");

        var getByCpfResponse = await _client.GetAsync($"/api/professores/cpf/{resultadoPost.Cpf}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, getByCpfResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_obterporCPF_e_deve_ser_Sucesso()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var getByCpfResponse = await _client.GetAsync($"/api/professores/cpf/{resultadoPost.Cpf}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getByCpfResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Atualizar_e_deve_ser_Sucesso()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var novoprofessor = DataFactory.ProfessorFaker.Generate();

        var dtoUpdate = new ProfessorDtoUpdate(
          ProfessorId: resultadoPost.ProfessorId,
          NomeCompleto: novoprofessor.NomeCompleto.Valor,
            DataNascimento: novoprofessor.DataNascimento.Valor,
            Email: novoprofessor.Email.Valor,
            Telefone: novoprofessor.Telefone.Valor,
            Salario: novoprofessor.Salario.Valor,
            Categoria: novoprofessor.Categoria.ToString()
        );
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var responseUpdate = await _client.PutAsJsonAsync($"/api/professores", dtoUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.OK, responseUpdate.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Atualizar_e_deve_ser_Falha()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var novoprofessor = DataFactory.ProfessorFaker.Generate();

        var dtoUpdate = new ProfessorDtoUpdate(
          ProfessorId: resultadoPost.ProfessorId,
          NomeCompleto: novoprofessor.NomeCompleto.Valor,
            DataNascimento: novoprofessor.DataNascimento.Valor,
            Email: novoprofessor.Email.Valor,
            Telefone: novoprofessor.Telefone.Valor,
            Salario: novoprofessor.Salario.Valor,
            Categoria: novoprofessor.Categoria.ToString()
        );
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante");

        var responseUpdate = await _client.PutAsJsonAsync($"/api/professores", dtoUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, responseUpdate.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Delete_e_deve_ser_Sucesso()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var delete = await _client.DeleteAsync($"/api/professores/{resultadoPost.ProfessorId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Delete_e_deve_ser_Falha()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Professor");
        var delete = await _client.DeleteAsync($"/api/professores/{resultadoPost.ProfessorId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Restaurar_e_deve_ser_Falha()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var delete = await _client.DeleteAsync($"/api/professores/{resultadoPost.ProfessorId}");

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Professor");
        var restuarar = await _client.PatchAsync
            ($"/api/professores/{resultadoPost.ProfessorId}/restaurar", new StringContent("", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, restuarar.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Restaurar_e_deve_ser_sucesso()
    {
        // Arrange
        var permisao = "Admin";
        var dto = Criarprrofessor();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", permisao);

        var postResponse = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var delete = await _client.DeleteAsync($"/api/professores/{resultadoPost.ProfessorId}");

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var restuarar = await _client.PatchAsync
            ($"/api/professores/{resultadoPost.ProfessorId}/restaurar", new StringContent("", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, restuarar.StatusCode);
    }
}