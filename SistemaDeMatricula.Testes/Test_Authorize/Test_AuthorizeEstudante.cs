using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Authorize;

[Collection("ApiMatrix")]
public class Test_AuthorizeEstudante : IntegrationTestBase, IAsyncLifetime
{
    public Test_AuthorizeEstudante(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Sucesso()
    {
        // Arrange
        var pemissao = "Admin";
        var estudanteFake = DataFactory.EstudanteFaker.Generate();

        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        // 🎯 AQUI: Vincula a permissão que você definiu ao cabeçalho da requisição
        // Limpa por garantia
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", pemissao);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        // Assert
        // Se o seu endpoint de criar estudante aceita a role "Estudante", ele deve retornar o sucesso esperado (ex: Created ou OK)
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Falha()
    {
        // Arrange
        var pemissao = "Estudante"; // Aqui você define a permissão que deseja testar
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        // 🎯 AQUI: Vincula a permissão que você definiu ao cabeçalho da requisição
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", pemissao);
        // Act
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        // Assert
        // Se o seu endpoint de criar estudante NÃO aceita a role "Estudante", ele deve retornar 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterPorId_Falha()
    {
        // Arrange
        var pemissao = "Professor"; // Aqui você define a permissão que deseja testar
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", pemissao);
        // Act
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterTodos_Sucesso()
    {
        // Arrange
        var pemissao = "Admin";
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", pemissao);
        // Act
        var response = await _client.GetAsync("/api/Estudante");
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterTodos_Falha()
    {
        // Arrange
        var pemissao = "Estudante"; // Faça isso em todos os métodos de teste para garantir o isolamento!
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", pemissao);
        // Act
        var response = await _client.GetAsync("/api/Estudante");
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_ObterPorId_Sucesso()
    {
        // Arrange
        var pemissao = "Admin";

        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", pemissao);

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();

        var idCriado = resultadoPost.EstudanteId;
        var getResponse = await _client.GetAsync($"/api/Estudante/{idCriado}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Atualizar_Sucesso()
    {
        var pemissao = "Admin";

        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", pemissao);

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 2. ACT
        var dataNascimentoAtualizada = estudanteFake.DataNascimento.Valor.AddYears(-1);
        var dtoUpdate = new EstudanteDtoUpdate(
            "Nome Atualizado",
            "email.atualizado@exemplo.com",
            dataNascimentoAtualizada,
            "(11) 99999-9999"
        );

        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idCriado}", dtoUpdate);

        // 3. ASSERT

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Atualizar_Falha()
    {
        // 1. ARRANGE: Criamos o cenário como ADMIN (Fluxo Feliz)
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin"); // Entra como Admin para preparar o terreno

        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);

        // Como entramos como Admin, agora o JSON existe (201 Created)!
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 🎯 O PULO DO GATO: Agora mudamos o crachá para o vilão do teste!
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante"); // Agora ele virou apenas Estudante

        var dataNascimentoAtualizada = estudanteFake.DataNascimento.Valor.AddYears(-1);
        var dtoUpdate = new EstudanteDtoUpdate(
            "Nome Atualizado",
            "email.atualizado@exemplo.com",
            dataNascimentoAtualizada,
            "(11) 99999-9999"
        );

        // 2. ACT: Tenta atualizar usando a permissão capada
        var putResponse = await _client.PutAsJsonAsync($"/api/Estudante/{idCriado}", dtoUpdate);

        // 3. ASSERT: Garante que foi barrado no PUT
        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Delete_Falha()
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin"); // Entra como Admin para preparar o terreno

        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);

        // Como entramos como Admin, agora o JSON existe (201 Created)!
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 🎯 O PULO DO GATO: Agora mudamos o crachá para o vilão do teste!
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante"); // Agora ele virou apenas Estudante
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idCriado}");
        // 3. ASSERT
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Test_AuthorizeEstudante_Delete_sucesso()
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin"); // Entra como Admin para preparar o terreno

        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        var postResponse = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);

        // Como entramos como Admin, agora o JSON existe (201 Created)!
        var resultadoPost = await postResponse.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
        var idCriado = resultadoPost.EstudanteId;

        // 🎯 O PULO DO GATO: Agora mudamos o crachá para o vilão do teste!
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin"); // Agora ele virou apenas Estudante
        var deleteResponse = await _client.DeleteAsync($"/api/Estudante/{idCriado}");
        // 3. ASSERT
        Assert.Equal(HttpStatusCode.NoContent
            , deleteResponse.StatusCode);
    }
}