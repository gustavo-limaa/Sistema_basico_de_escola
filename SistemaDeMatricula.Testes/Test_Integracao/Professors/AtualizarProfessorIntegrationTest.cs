using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class AtualizarProfessorIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public AtualizarProfessorIntegrationTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // 1. ANTES DE CADA TESTE: Não precisamos de nada especial aqui
    public Task InitializeAsync() => Task.CompletedTask;

    // 2. DEPOIS DE CADA TESTE: Aqui é onde a mágica da limpeza acontece
    public async Task DisposeAsync()
    {
        // Criamos um "escopo" para conseguir pegar o AppDbContext lá de dentro da API
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Agora sim! O 'contexto' existe aqui e podemos limpar a tabela
        await contexto.Professores.ExecuteDeleteAsync();
    }

    // 1. A Fonte da Verdade: Gera o objeto do Faker que contém os dados
    private Professor GerarProfessorAleatorio()
        => DataFactory.ProfessorFaker.Generate();

    // 2. O DTO de Criação usa a Fonte da Verdade
    private ProfessorDtoCreate CriarDtoValido(string? emailSobrescrito = null)
    {
        var p = GerarProfessorAleatorio();

        return new ProfessorDtoCreate(
           NomeCompleto: p.NomeCompleto.Valor,
           Cpf: p.Cpf.Valor,
           DataNascimento: p.DataNascimento.Valor,
           Email: emailSobrescrito ?? p.Email.Valor,
           Telefone: p.Telefone.Valor,
           Salario: p.Salario.Valor,
           Categoria: p.Categoria.ToString()
        );
    }

    // 3. O DTO de Update também usa a Fonte da Verdade
    private ProfessorDtoUpdate CriarDtoUpdate(Guid idExistente, string? emailSobrescrito = null)
    {
        var p = GerarProfessorAleatorio();

        return new ProfessorDtoUpdate(
            ProfessorId: idExistente,
            NomeCompleto: p.NomeCompleto.Valor,
            DataNascimento: p.DataNascimento.Valor,
            Email: emailSobrescrito ?? p.Email.Valor,
            Telefone: p.Telefone.Valor,
            Salario: p.Salario.Valor,
            Categoria: p.Categoria.ToString()
        );
    }

    [Fact]
    public async Task Ciclo_Completo_Criar_e_Atualizar()
    {
        // 1. Cria um DTO de post e envia
        var dtoPost = CriarDtoValido();
        var responsePost = await _client.PostAsJsonAsync("/api/professores", dtoPost);
        var criado = await responsePost.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Interligação: Gera um DTO de Update usando o ID do que acabou de ser criado
        var dtoUpdate = CriarDtoUpdate(criado.ProfessorId);

        // 3. Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);

        // 4. Assert
        responsePut.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Atualizar_Professor_Inexistente_Retorna_NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var dtoUpdate = CriarDtoUpdate(idInexistente);
        // Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);
        // Assert
        responsePut.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_Professor_Com_Email_De_Outro_Retorna_Conflict()
    {
        // 1. Arrange: Cria dois professores VÁLIDOS e DIFERENTES
        var response1 = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        var profA = await response1.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var response2 = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        var profB = await response2.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Tenta atualizar o Professor B usando o E-mail do Professor A
        var dtoUpdate = CriarDtoUpdate(profB.ProfessorId, emailSobrescrito: profA.Email);

        // 3. Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);

        // 4. Assert
        responsePut.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Atualizar_Professor_Com_Id_Invalido_Retorna_BadRequest()
    {
        // Arrange: Enviamos um objeto anônimo com uma string que não é GUID
        var dtoInvalido = new
        {
            ProfessorId = "nao-sou-um-guid",
            NomeCompleto = "Teste",
            DataNascimento = "1999-01-01",
            Email = "test@example.com",
            Telefone = "11999999999",
            Salario = 5000.00,
            Categoria = "Titular"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/professores", dtoInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Atualizar_Professor_Mantendo_Mesmo_Email_Deve_Retornar_Ok()
    {
        // 1. Arrange: Cria um professor original no banco
        var dtoCriacao = CriarDtoValido();
        var responsePost = await _client.PostAsJsonAsync("/api/professores", dtoCriacao);
        var professorCriado = await responsePost.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Preparamos o Update: Mudamos TUDO, menos o E-mail
        // Usamos o método auxiliar passando o e-mail que já está lá no banco
        var emailOriginal = professorCriado.Email;
        var dtoUpdate = CriarDtoUpdate(professorCriado.ProfessorId, emailSobrescrito: emailOriginal);

        // Garantimos que os outros dados são diferentes (Nome, Telefone, etc virão novos do Bogus)
        // O CPF nem entra aqui, como a gente já sabe!

        // 3. Act
        var responsePut = await _client.PutAsJsonAsync("/api/professores", dtoUpdate);
        var professorAtualizado = await responsePut.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 4. Assert
        responsePut.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Verificamos se o e-mail continuou o mesmo
        professorAtualizado.Email.Should().Be(emailOriginal);

        // Verificamos se o nome mudou (provando que a atualização ocorreu)
        professorAtualizado.NomeCompleto.Should().Be(dtoUpdate.NomeCompleto);
        professorAtualizado.NomeCompleto.Should().NotBe(dtoCriacao.NomeCompleto);
    }
}