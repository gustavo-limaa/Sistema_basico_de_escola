using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Professors;

[Collection("ApiMatrix")]
public class SoftDeleteProfessorIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public SoftDeleteProfessorIntegrationTest(SistemaMatriculaFactory factory)
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

    // 1. Auxiliar de Arrange: Cria um professor e já te entrega o ID pronto para ser deletado
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

    private async Task<Guid> CadastrarProfessorERetornarIdAsync()
    {
        var dto = CriarDtoValido();
        var response = await _client.PostAsJsonAsync("/api/professores", dto);
        var criado = await response.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        return criado!.ProfessorId;
    }

    // 2. Auxiliar de Assert: O "Raio-X" que ignora o filtro global para ver se o dado ainda existe
    private async Task<bool> VerificarSeProfessorEstaInativoNoBanco(Guid id)
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // IgnoreQueryFilters é a chave para ver os "fantasmas"
        var professor = await contexto.Professores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProfessorId == id);

        // Retorna true apenas se ele existir E o campo Ativo for false
        return professor != null && !professor.Ativo;
    }

    [Fact]
    public async Task Deletar_Professor_Deve_Mudar_Status_Para_Inativo_No_Banco()
    {
        // 1. Arrange: Usa o auxiliar para já ter um ID válido no banco
        var idParaDeletar = await CadastrarProfessorERetornarIdAsync();

        // 2. Act: Executa o Delete
        var responseDelete = await _client.DeleteAsync($"/api/professores/{idParaDeletar}");

        // 3. Assert
        responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent); // 204

        // 4. Verificação de Superfície: O GET normal não deve achar ele (404)
        var responseGet = await _client.GetAsync($"/api/professores/{idParaDeletar}");
        responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 5. Verificação de Subsolo: Usa o Raio-X para confirmar que ele virou "Inativo"
        var estaInativo = await VerificarSeProfessorEstaInativoNoBanco(idParaDeletar);
        estaInativo.Should().BeTrue();
    }

    [Fact]
    public async Task Deletar_Professor_Inexistente_Deve_Retornar_NotFound()
    {
        // Arrange: Geramos um ID aleatório que não existe no banco
        var idInexistente = Guid.NewGuid();
        // Act: Tentamos deletar esse ID
        var responseDelete = await _client.DeleteAsync($"/api/professores/{idInexistente}");
        // Assert: Esperamos um 404 Not Found
        responseDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deletar_Professor_Ja_Inativo_Deve_Retornar_NotFound()
    {
        // Arrange: Criamos um professor, deletamos ele (ficando inativo) e depois tentamos deletar de novo
        var id = await CadastrarProfessorERetornarIdAsync();
        await _client.DeleteAsync($"/api/professores/{id}"); // Primeiro delete para deixar inativo
        // Act: Tentamos deletar o mesmo ID novamente
        var responseDeleteNovamente = await _client.DeleteAsync($"/api/professores/{id}");
        // Assert: Esperamos um 404 Not Found, pois ele já está "inativo"
        responseDeleteNovamente.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deletar_Professor_Ja_Inativo_Deve_Retornar_NotFound_2()
    {
        // Arrange: Criamos um professor, deletamos ele (ficando inativo) e depois tentamos deletar de novo
        var id = await CadastrarProfessorERetornarIdAsync();
        await _client.DeleteAsync($"/api/professores/{id}"); // Primeiro delete para deixar inativo
        // Act: Tentamos deletar o mesmo ID novamente
        var responseDeleteNovamente = await _client.DeleteAsync($"/api/professores/{id}");
        // Assert: Esperamos um 404 Not Found, pois ele já está "inativo"
        responseDeleteNovamente.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deletar_Professor_Com_Id_Invalido_Deve_Retornar_BadRequest()
    {
        // Arrange: Usamos um ID que não é um GUID válido
        var idInvalido = "12345";
        // Act: Tentamos deletar usando esse ID inválido
        var responseDelete = await _client.DeleteAsync($"/api/professores/{idInvalido}");
        // Assert: Esperamos um 400 Bad Request, pois o formato do ID é inválido
        responseDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deletar_Professor_E_Verificar_Se_Ele_Nao_Aparece_Mais_Em_Lista_De_Todos()
    {
        // Arrange: Criamos um professor e o deletamos
        var id = await CadastrarProfessorERetornarIdAsync();
        await _client.DeleteAsync($"/api/professores/{id}"); // Deleta para ficar inativo
        // Act: Buscamos a lista de todos os professores
        var responseGetTodos = await _client.GetAsync("/api/professores");
        var listaProfessores = await responseGetTodos.Content.ReadFromJsonAsync<List<ProfessorDtoResponse>>();
        // Assert: O professor deletado (inativo) não deve aparecer na lista de todos
        listaProfessores.Should().NotContain(p => p.ProfessorId == id);
    }

    [Fact]
    public async Task Deletar_Professor_Deve_Sumir_Da_Api_Mas_Continuar_Inativo_No_Banco()
    {
        var id = await CadastrarProfessorERetornarIdAsync();

        // 2. Act: Deleta via API (ADICIONE O $ AQUI)
        var responseDelete = await _client.DeleteAsync($"/api/professores/{id}");

        // ... restante do código ...

        // 3. Assert (Superfície): (ADICIONE O $ AQUI TAMBÉM)
        var responseGet = await _client.GetAsync($"/api/professores/{id}");
        responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 4. Assert (Subsolo): Vamos ver se ele ainda existe no banco
        // Criamos um escopo para acessar o banco "por trás" da API
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // A MÁGICA: IgnoreQueryFilters() permite ver o que está Ativo = false
        var professorNoBanco = await contexto.Professores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProfessorId == id);

        // Validações finais
        professorNoBanco.Should().NotBeNull(); // Ele NÃO foi excluído do HD
        professorNoBanco!.Ativo.Should().BeFalse(); // Mas ele está INATIVO
    }
}