using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;

// No seu método, você usa o apelido:

namespace SistemaDeMatricula.Testes.Test_Integracao.Matriculas;

[Collection("ApiMatrix")]
public class CriarMatriculaIntegrationTest : IntegrationTestBase, IAsyncLifetime

{
    public CriarMatriculaIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await DataFactory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Criar_Matricula_com_Sucesso()
    {
        var (_, turma, _) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado

        // Cria um NOVO estudante que ainda não está na turma
        var novoEstudante = DataFactory.EstudanteFaker.Generate();

        // Salva ele no banco (precisa do scope aqui se não moveu para o construtor)
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexto.Estudantes.AddAsync(novoEstudante);
        await contexto.SaveChangesAsync();

        var dto = new MatriculaDtoCreate(novoEstudante.Id, turma.Id);
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK); // Ou 201 Created
    }

    [Fact]
    public async Task Criar_Matricula_com_falha()
    {
        // Bem mais limpo!
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        var dto = new MatriculaDtoCreate(Guid.NewGuid(), turma.Id);
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_Matricula_com_falha_por_misclick()
    {
        // Bem mais limpo!
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        var dto = new MatriculaDtoCreate(estudante.Id, turma.Id);
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        var post2 = await _client.PostAsJsonAsync("/api/matriculas", postResponse);

        post2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Turma_Estiver_Lotada()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Criamos um cenário onde a turma só tem 1 vaga
        var (alun, turma, matricula) = await DataFactory.CriarCenarioDeMatriculaValido(contexto, capacidade: 1);

        // CONFERÊNCIA:
        var contagemNoBanco = await contexto.Matriculas.CountAsync(m => m.TurmaId == turma.Id && m.Ativo);
        // Se isso aqui for 0, o erro está no DataFactory (não salvou ou salvou inativo)
        contagemNoBanco.Should().Be(1);
        // Aluno 2 tenta entrar na mesma turma
        var aluno2 = DataFactory.EstudanteFaker.Generate();
        await contexto.Estudantes.AddAsync(aluno2);
        await contexto.SaveChangesAsync();

        var dto = new MatriculaDtoCreate(aluno2.Id, turma.Id);
        var response = await _client.PostAsJsonAsync("/api/matriculas", dto);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        // Opcional: verificar se a mensagem é "Turma lotada"
    }
}