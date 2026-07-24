using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

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
        return await Data_Factory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Criar_Matricula_com_Sucesso()
    {
        var (_, turma, _) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado

        // Cria um NOVO estudante que ainda não está na turma
        var novoEstudante = Data_Factory.EstudanteFaker.Generate();

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
        // ARANGE
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var disciplina = Data_Factory.DisciplinaFaker.Generate();
        var professor = Data_Factory.ProfessorFaker.Generate();
        contexto.Disciplinas.Add(disciplina);
        contexto.Professores.Add(professor);
        await contexto.SaveChangesAsync();

        var turma = Data_Factory.TurmaFaker(professor.Id, disciplina.Id, 1).Generate();
        contexto.Turmas.Add(turma);
        await contexto.SaveChangesAsync();

        var aluno1 = Data_Factory.EstudanteFaker.Generate();
        contexto.Estudantes.Add(aluno1);
        await contexto.SaveChangesAsync();

        var matricula1 = new Matricula(aluno1.Id, turma.Id);
        contexto.Matriculas.Add(matricula1);
        await contexto.SaveChangesAsync();

        var aluno2 = Data_Factory.EstudanteFaker.Generate();
        contexto.Estudantes.Add(aluno2);
        await contexto.SaveChangesAsync();

        var dto = new MatriculaDtoCreate(aluno2.Id, turma.Id);

        // ACT
        var response = await _client.PostAsJsonAsync("/api/matriculas", dto);

        // ASSERT
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}