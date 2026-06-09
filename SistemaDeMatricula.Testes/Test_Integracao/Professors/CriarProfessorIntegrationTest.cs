using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class CriarProfessorIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public CriarProfessorIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private ProfessorDtoCreate CriarDtoValido()
    {
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = new ProfessorDtoCreate(

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
    public async Task Criar_Professor_Retorna_Professor_Criado()
    {
        // Arrange
        var dto = CriarDtoValido();

        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultado = await response.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task Criar_Professor_Com_Cpf_Existente_Retorna_Conflict()
    {
        // Arrange
        var dto = CriarDtoValido();
        // Primeiro criamos o professor normalmente
        var response1 = await _client.PostAsJsonAsync("/api/professores", dto);
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        // Agora tentamos criar outro com o mesmo CPF
        var response2 = await _client.PostAsJsonAsync("/api/professores", dto);
        // Assert
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Criar_Professor_Com_Email_Existente_Retorna_Conflict()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = new ProfessorDtoCreate(
            NomeCompleto: professor.NomeCompleto.Valor,
            Cpf: professor.Cpf.Valor,
            DataNascimento: professor.DataNascimento.Valor,
            Email: professor.Email.Valor,
            Telefone: professor.Telefone.Valor,
            Salario: professor.Salario.Valor,
            Categoria: professor.Categoria.ToString()
          );
        // Primeiro criamos o professor normalmente
        var response1 = await _client.PostAsJsonAsync("/api/professores", dto);
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        // Agora tentamos criar outro com o mesmo Email
        var professor2 = DataFactory.ProfessorFaker.Generate();
        var dto2 = new ProfessorDtoCreate(
            NomeCompleto: professor2.NomeCompleto.Valor,
            Cpf: professor2.Cpf.Valor, // CPF diferente para não conflitar por CPF
            DataNascimento: professor2.DataNascimento.Valor,
            Email: professor.Email.Valor, // Mesmo email para testar conflito
            Telefone: professor2.Telefone.Valor,
            Salario: professor2.Salario.Valor,
            Categoria: professor2.Categoria.ToString()
          );
        var response2 = await _client.PostAsJsonAsync("/api/professores", dto2);
        // Assert
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Criar_Professor_Com_Dados_Invalidos_Retorna_BadRequest()
    {
        // Arrange
        var dto = new ProfessorDtoCreate(
            NomeCompleto: "", // Nome vazio
            Cpf: "123", // CPF inválido
            DataNascimento: DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), // Data de nascimento muito recente
            Email: "email-invalido", // Email sem formato correto
            Telefone: "abc", // Telefone inválido
            Salario: -1000, // Salário negativo
            Categoria: "CategoriaInvalida" // Categoria que não existe
          );
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Salario_Negativo_Retorna_BadRequest()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = new ProfessorDtoCreate(
            NomeCompleto: professor.NomeCompleto.Valor,
            Cpf: professor.Cpf.Valor,
            DataNascimento: professor.DataNascimento.Valor,
            Email: professor.Email.Valor,
            Telefone: professor.Telefone.Valor,
            Salario: -5000, // Salário negativo para testar validação
            Categoria: professor.Categoria.ToString()
          );
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Categoria_Invalida_Retorna_BadRequest()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = new ProfessorDtoCreate(
            NomeCompleto: professor.NomeCompleto.Valor,
            Cpf: professor.Cpf.Valor,
            DataNascimento: professor.DataNascimento.Valor,
            Email: professor.Email.Valor,
            Telefone: professor.Telefone.Valor,
            Salario: professor.Salario.Valor,
            Categoria: "CategoriaInvalida" // Categoria que não existe para testar validação
          );
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Data_Nascimento_Futura_Retorna_BadRequest()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = new ProfessorDtoCreate(
            NomeCompleto: professor.NomeCompleto.Valor,
            Cpf: professor.Cpf.Valor,
            DataNascimento: DateOnly.FromDateTime(DateTime.Now.AddYears(1)), // Data de nascimento no futuro
            Email: professor.Email.Valor,
            Telefone: professor.Telefone.Valor,
            Salario: professor.Salario.Valor,
            Categoria: professor.Categoria.ToString()
          );
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Professor_Com_Nome_Vazio_Retorna_BadRequest()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = new ProfessorDtoCreate(
            NomeCompleto: "", // Nome vazio para testar validação
            Cpf: professor.Cpf.Valor,
            DataNascimento: professor.DataNascimento.Valor,
            Email: professor.Email.Valor,
            Telefone: professor.Telefone.Valor,
            Salario: professor.Salario.Valor,
            Categoria: professor.Categoria.ToString()
          );
        // Act
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}