using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Test.Shared; // 🚀 Namespace correto da Shared
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace SistemaDeMatricula.Testes.Test_Authorize;

public class Test_AuthorizeProfessor : PermissaoTestBase // 🎯 Herda da nova classe base de segurança
{
    public Test_AuthorizeProfessor(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_EndpointProtegido_Retorna201()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        // Act
        var post = await _Client.PostAsJsonAsync("/api/professores", dto);

        // Assert
        post.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_EndpointProtegido_Deve_Falhar()
    {
        // Arrange
        AutenticarComoEstudante(); // 🎯 Estudante não pode criar professores
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        // Act
        var post = await _Client.PostAsJsonAsync("/api/professores", dto);

        // Assert
        post.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_ObterTodos_Deve_Falhar()
    {
        // Arrange
        AutenticarComoEstudante();

        // Act
        var response = await _Client.GetAsync("/api/professores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_ObterTodos_Deve_Ser_Sucesso()
    {
        // Arrange
        AutenticarComoProfessor();

        // Act
        var response = await _Client.GetAsync("/api/professores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_ObterPorId_Deve_Ser_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        AutenticarComoProfessor();
        var getByIdResponse = await _Client.GetAsync($"/api/professores/{resultadoPost!.ProfessorId}");

        // Assert
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_ObterPorId_Deve_Ser_Falha()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        AutenticarComoEstudante();
        var getByIdResponse = await _Client.GetAsync($"/api/professores/{resultadoPost!.ProfessorId}");

        // Assert
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_ObterPorCpf_Deve_Ser_Falha()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        AutenticarComoEstudante();
        var getByCpfResponse = await _Client.GetAsync($"/api/professores/cpf/{resultadoPost!.Cpf}");

        // Assert
        getByCpfResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_ObterPorCpf_Deve_Ser_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        ResetarParaAdmin();
        var getByCpfResponse = await _Client.GetAsync($"/api/professores/cpf/{resultadoPost!.Cpf}");

        // Assert
        getByCpfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Atualizar_Deve_Ser_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var novoprofessor = Data_Factory.ProfessorFaker.Generate();
        var dtoUpdate = new ProfessorDtoUpdate(
            ProfessorId: resultadoPost!.ProfessorId,
            NomeCompleto: novoprofessor.NomeCompleto.Valor,
            DataNascimento: novoprofessor.DataNascimento.Valor,
            Email: novoprofessor.Email.Valor,
            Telefone: novoprofessor.Telefone.Valor,
            Salario: novoprofessor.Salario.Valor,
            Categoria: novoprofessor.Categoria.ToString()
        );

        // Act
        ResetarParaAdmin();
        var responseUpdate = await _Client.PutAsJsonAsync("/api/professores", dtoUpdate);

        // Assert
        responseUpdate.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Atualizar_Deve_Ser_Falha()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var novoprofessor = Data_Factory.ProfessorFaker.Generate();
        var dtoUpdate = new ProfessorDtoUpdate(
            ProfessorId: resultadoPost!.ProfessorId,
            NomeCompleto: novoprofessor.NomeCompleto.Valor,
            DataNascimento: novoprofessor.DataNascimento.Valor,
            Email: novoprofessor.Email.Valor,
            Telefone: novoprofessor.Telefone.Valor,
            Salario: novoprofessor.Salario.Valor,
            Categoria: novoprofessor.Categoria.ToString()
        );

        // Act
        AutenticarComoEstudante();
        var responseUpdate = await _Client.PutAsJsonAsync("/api/professores", dtoUpdate);

        // Assert
        responseUpdate.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Delete_Deve_Ser_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        ResetarParaAdmin();
        var delete = await _Client.DeleteAsync($"/api/professores/{resultadoPost!.ProfessorId}");

        // Assert
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Delete_Deve_Ser_Falha()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        AutenticarComoProfessor(); // Professor tenta deletar outro professor
        var delete = await _Client.DeleteAsync($"/api/professores/{resultadoPost!.ProfessorId}");

        // Assert
        delete.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Restaurar_Deve_Ser_Falha()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        ResetarParaAdmin();
        await _Client.DeleteAsync($"/api/professores/{resultadoPost!.ProfessorId}");

        // Act
        AutenticarComoProfessor();
        var restaurar = await _Client.PatchAsJsonAsync($"/api/professores/{resultadoPost.ProfessorId}/restaurar", new { });

        // Assert
        restaurar.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeProfessor_Restaurar_Deve_Ser_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();
        var dto = Data_Factory.ProfessorFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/professores", dto);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        ResetarParaAdmin();
        await _Client.DeleteAsync($"/api/professores/{resultadoPost!.ProfessorId}");

        // Act
        ResetarParaAdmin();
        var restaurar = await _Client.PatchAsJsonAsync($"/api/professores/{resultadoPost.ProfessorId}/restaurar", new { });

        // Assert
        restaurar.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}