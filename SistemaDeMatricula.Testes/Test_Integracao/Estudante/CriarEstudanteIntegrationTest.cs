using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Estudante;

[Collection("ApiMatrix")]
public class CriarEstudanteIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public CriarEstudanteIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Criar_Estudante_Valido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, $"A API recusou o estudante! Motivo: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            "",
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Email_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            "email-invalido",
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com email inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_CPF_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            "12345678900",
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com CPF inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Telefone_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            "12345"
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com telefone inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_DataNascimento_Futura()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();

        var amanha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            amanha,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com data futura! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Nome_Excessivamente_Longo()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var nomeMuitoLongo = new string('A', 256);
        var dtoCreate = new EstudanteDtoCreate(
            nomeMuitoLongo,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com nome excessivamente longo! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Email_Excessivamente_Longo()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var emailMuitoLongo = new string('A', 256) + "@exemplo.com";
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            emailMuitoLongo,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com email excessivamente longo! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_CPF_Excessivamente_Longo()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var cpfMuitoLongo = new string('1', 12);
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            cpfMuitoLongo,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com CPF excessivamente longo! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Telefone_Excessivamente_Longo()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var telefoneMuitoLongo = new string('1', 16);
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            telefoneMuitoLongo
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com telefone excessivamente longo! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_DataNascimento_Excessivamente_Antiga()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dataNascimentoMuitoAntiga = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-150));
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            dataNascimentoMuitoAntiga,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com data de nascimento excessivamente antiga! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_erro_criar_estudante_com_cpf_duplicado()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response1 = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        Assert.True(response1.IsSuccessStatusCode, "A API recusou o primeiro estudante! Algo está muito errado.");
        var response2 = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response2.Content.ReadAsStringAsync();
        Assert.False(response2.IsSuccessStatusCode, $"A API aceitou um estudante com CPF duplicado! Resposta: {respostaDaApi}");
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }
}