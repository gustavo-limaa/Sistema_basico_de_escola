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

        // ATENÇÃO: Verifique se essa é a rota real da sua Controller!
        // Mude de "/api/estudantes" para "/api/Estudante"
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        // 3. LÊ A FOFOCA INTEIRA DA API
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        // 4. ASSERT COM MENSAGEM CUSTOMIZADA
        // Se o status não for sucesso (200-299), o teste quebra e joga a mensagem real na sua cara!
        Assert.True(response.IsSuccessStatusCode, $"A API recusou o estudante! Motivo: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            "", // NOME VAZIO (INVÁLIDO)
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
            "email-invalido", // EMAIL INVÁLIDO
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
            "12345678900", // CPF INVÁLIDO
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
            "12345" // TELEFONE INVÁLIDO
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com telefone inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_DataNascimento_Futura()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();

        // Pega o dia de amanhã direto do relógio, sem conversão de texto
        var amanha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            amanha, // Passa o objeto DateOnly puro
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
        var nomeMuitoLongo = new string('A', 256); // Supondo que o limite seja 255 caracteres
        var dtoCreate = new EstudanteDtoCreate(
            nomeMuitoLongo, // NOME EXCESSIVAMENTE LONGO
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
        var emailMuitoLongo = new string('A', 256) + "@exemplo.com"; // Supondo que o limite seja 255 caracteres
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            emailMuitoLongo, // EMAIL EXCESSIVAMENTE LONGO
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
        var cpfMuitoLongo = new string('1', 12); // Supondo que o limite seja 11 caracteres
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            cpfMuitoLongo, // CPF EXCESSIVAMENTE LONGO
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
        var telefoneMuitoLongo = new string('1', 16); // Supondo que o limite seja 15 caracteres
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            telefoneMuitoLongo // TELEFONE EXCESSIVAMENTE LONGO
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com telefone excessivamente longo! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_DataNascimento_Excessivamente_Antiga()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dataNascimentoMuitoAntiga = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-150)); // Supondo que o limite seja 120 anos atrás
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            dataNascimentoMuitoAntiga, // DATA DE NASCIMENTO EXCESSIVAMENTE ANTIGA
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
        // Primeiro, criamos um estudante normalmente
        var response1 = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        Assert.True(response1.IsSuccessStatusCode, "A API recusou o primeiro estudante! Algo está muito errado.");
        // Agora, tentamos criar outro estudante com o mesmo CPF
        var response2 = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response2.Content.ReadAsStringAsync();
        Assert.False(response2.IsSuccessStatusCode, $"A API aceitou um estudante com CPF duplicado! Resposta: {respostaDaApi}");
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        // Ou BadRequest, se a sua API estiver configurada assim.
    }
}