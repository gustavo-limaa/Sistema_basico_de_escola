using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class PegarTodosEPegarPorIdProfessorIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public PegarTodosEPegarPorIdProfessorIntegrationTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // 1. ANTES DE CADA TESTe: Aqui é onde a mágica da preparação acontece
    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Geramos 10 professores usando o seu Bogus
        var listaProfessores = DataFactory.ProfessorFaker.Generate(10);

        // 2. "Macete": Desativamos 3 professores da lista
        // Isso serve para testar se o GET ignora os inativos automaticamente
        listaProfessores[0].Desativar();
        listaProfessores[1].Desativar();
        listaProfessores[2].Desativar();

        // 3. Salvamos no banco de teste
        await contexto.Professores.AddRangeAsync(listaProfessores);
        await contexto.SaveChangesAsync();
    }

    // 2. DEPOIS DE CADA TESTE: Aqui é onde a mágica da limpeza acontece
    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Guarde o total de deletados para ver no log
        int deletados = await contexto.Professores.ExecuteDeleteAsync();

        // Se deletados for 0 e você sabe que tinha dados, o contexto está apontando
        // para um banco diferente do que a API está usando.
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
    public async Task Pegar_Todos_Professores_Retorna_Lista_Com_Professores_Ativos()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Se tiver um helper de limpeza
        }
        var response = await _client.GetAsync("/api/professores ");
        var professores = await response.Content.ReadFromJsonAsync<List<ProfessorDtoResponse>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        professores.Should().NotBeNull();
        professores.Count.Should().Be(7); // 10 - 3 (desativados)
    }

    [Fact]
    public async Task Pegar_Professor_Por_Id_Retorna_Professor_Se_Existe_E_Ativo()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = CriarDtoValido();
        var response1 = await _client.PostAsJsonAsync("/api/professores", dto);
        var resultado = await response1.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Act
        var response2 = await _client.GetAsync($"/api/professores/{resultado.ProfessorId}");
        var professorEncontrado = await response2.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // Assert
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        professorEncontrado.Should().NotBeNull();
    }

    [Fact]
    public async Task Pegar_Professor_Por_Id_Retorna_NotFound_Se_Professor_Nao_Existir()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        // Act
        var response = await _client.GetAsync($"/api/professores/{idInexistente}");
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Pegar_Professor_Por_Id_Retorna_BadRequest_Se_Id_For_Invalido()
    {
        // Arrange
        var idInvalido = "123"; // Não é um GUID válido
        // Act
        var response = await _client.GetAsync($"/api/professores/{idInvalido}");
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Restaurar_Professor_Desativado_Deve_Voltar_A_Ser_Exibido_No_Get()
    {
        // 1. Arrange: Criamos e desativamos um professor
        var dto = CriarDtoValido();
        var responsePost = await _client.PostAsJsonAsync("/api/professores", dto);
        var criado = await responsePost.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        await _client.DeleteAsync($"/api/professores/{criado.ProfessorId}");

        // Garantimos que ele está "invisível" (404)
        var responseGetInativo = await _client.GetAsync($"/api/professores/{criado.ProfessorId}");
        responseGetInativo.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        // 2. Act: Chamamos a restauração (PATCH)
        var responseRestore = await _client.PatchAsync($"/api/professores/{criado.ProfessorId}/restaurar", null);
        // No seu teste:

        if (responseRestore.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            var erroDetalhado = await responseRestore.Content.ReadAsStringAsync();
            // Isso vai aparecer no "Output" do seu teste no Visual Studio
            throw new Exception($"ERRO 500 DETALHADO: {erroDetalhado}");
        }

        // 3. Assert: A prova real - O GET agora tem que retornar 200 OK
        var responseGetAtivo = await _client.GetAsync($"/api/professores/{criado.ProfessorId}");
        responseGetAtivo.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var restaurado = await responseGetAtivo.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        restaurado.ProfessorId.Should().Be(criado.ProfessorId);
        restaurado.NomeCompleto.Should().Be(dto.NomeCompleto);
    }
}