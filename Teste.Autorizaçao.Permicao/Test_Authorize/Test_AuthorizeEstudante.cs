using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Test.Shared; // 🚀 Garante o underline da Shared certa
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Authorize;

public class Test_AuthorizeEstudante : PermissaoTestBase
{
    public Test_AuthorizeEstudante(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Sucesso()
    {
        // Arrange
        ResetarParaAdmin(); // Injeta o crachá que tem permissão para cadastrar
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        // Act
        var response = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Falha()
    {
        // Arrange
        AutenticarComoEstudante();
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        // Act
        var response = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterPorId_Falha()
    {
        // Arrange
        AutenticarComoProfessor();
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        // Act
        var response = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterTodos_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();

        // Act
        var response = await _Client.GetAsync("/api/Estudante");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterTodos_Falha()
    {
        // Arrange
        AutenticarComoEstudante();

        // Act
        var response = await _Client.GetAsync("/api/Estudante");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterPorId_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost!.EstudanteId;

        // Act
        var getResponse = await _Client.GetAsync($"/api/Estudante/{idCriado}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Atualizar_Sucesso()
    {
        // Arrange
        ResetarParaAdmin();
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost!.EstudanteId;

        var dtoUpdate = Data_Factory.EstudanteFakerup.Generate();
        // Act
        var putResponse = await _Client.PutAsJsonAsync($"/api/Estudante/{idCriado}", dtoUpdate);

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Atualizar_Falha()
    {
        // Arrange
        ResetarParaAdmin();
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost!.EstudanteId;

        // Act
        AutenticarComoEstudante();

        var dtoUpdate = Data_Factory.EstudanteFakerup.Generate();

        var putResponse = await _Client.PutAsJsonAsync($"/api/Estudante/{idCriado}", dtoUpdate);

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Delete_Falha()
    {
        // Arrange: 👑 Prepara o terreno como Admin
        ResetarParaAdmin();
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost!.EstudanteId;

        // Act: 🎯 Altera para estudante para testar a negação do delete
        AutenticarComoEstudante();
        var deleteResponse = await _Client.DeleteAsync($"/api/Estudante/{idCriado}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Delete_sucesso()
    {
        ResetarParaAdmin();
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var postResponse = await _Client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost!.EstudanteId;

        // Act: Mantém ou força o Admin para validar o sucesso da deleção
        ResetarParaAdmin();
        var deleteResponse = await _Client.DeleteAsync($"/api/Estudante/{idCriado}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}