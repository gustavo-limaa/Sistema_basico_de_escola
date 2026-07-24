using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
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
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Invalido()
    {
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { Telefone = "12382314" };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Email_Invalido()
    {
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { Email = "asdeweasdacaca" };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_CPF_Invalido()
    {
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { Cpf = "1239012094184" };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Telefone_Invalido()
    {
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { Telefone = "12382314" };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_DataNascimento_Futura()
    {
        var amanha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { DataNascimento = amanha };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Nome_Excessivamente_Longo()
    {
        var nomeMuitoLongo = new string('A', 256);

        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { NomeCompleto = nomeMuitoLongo };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Email_Excessivamente_Longo()
    {
        var emailMuitoLongo = new string('A', 256) + "@exemplo.com";
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { Email = emailMuitoLongo };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_CPF_Excessivamente_Longo()
    {
        var cpfMuitoLongo = new string('1', 12);
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invalido = dtoCreate with { Cpf = cpfMuitoLongo };

        var response = await _client.PostAsJsonAsync("/api/Estudante", invalido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_DataNascimento_Excessivamente_Antiga()
    {
        var dataNascimentoMuitoAntiga = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-150));
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();
        var invallido = dtoCreate with { DataNascimento = dataNascimentoMuitoAntiga };
        var response = await _client.PostAsJsonAsync("/api/Estudante", invallido);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, MensagensEstudante.ErroAoCriarEstudante);
    }

    [Fact]
    public async Task Deve_Retornar_erro_criar_estudante_com_cpf_duplicado()
    {
        var dtoCreate = Data_Factory.EstudanteFakerdto.Generate();

        var response1 = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        Assert.True(response1.IsSuccessStatusCode);

        var response2 = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response2.Content.ReadAsStringAsync();

        // ASSERT
        response2.IsSuccessStatusCode.Should().BeFalse();
        respostaDaApi.Should().Contain(MensagensEstudante.ErroDeDuplicidade);
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }
}