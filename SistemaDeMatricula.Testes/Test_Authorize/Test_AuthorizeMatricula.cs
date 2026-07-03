using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Authorize;

[Collection("ApiMatrix")]
public class Test_AuthorizeMatricula : IntegrationTestBase, IAsyncLifetime
{
    public Test_AuthorizeMatricula(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await DataFactory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Criar_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        //ARRANGE

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

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
    public async Task Criar_Matricula_Sem_Autorizacao_Deve_SER_Falha()
    {
        //ARRANGE

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante");

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

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden); // Ou 403 Forbidden
    }

    [Fact]
    public async Task Obterporid_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        //ARRANGE

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var (estudante, turma, matricula) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado

        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}");

        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK); // Ou 200 OK
    }

    [Fact]
    public async Task Obterporid_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        //ARRANGE

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "estudante");

        var (estudante, turma, matricula) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado

        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}");

        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden); // Ou 200 OK
    }

    [Fact]
    public async Task Obtertodos_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        //ARRANGE

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var (estudante, turma, matricula) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado

        var getResponse = await _client.GetAsync($"/api/matriculas/");

        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK); // Ou 200 OK
    }

    [Fact]
    public async Task Obtertodos_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        //ARRANGE

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "estudante");

        var (estudante, turma, matricula) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado

        var getResponse = await _client.GetAsync($"/api/matriculas/");

        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden); // Ou 200 OK
    }

    [Fact]
    public async Task Delete_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        //ARRANGE
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();
        //act
        var deleteResponse = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");
        // assert
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK); // Ou 200 OK
    }

    [Fact]
    public async Task Delete_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        //ARRANGE
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante");
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();
        //act
        var deleteResponse = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");
        // assert
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden); // Ou 403 Forbidden
    }

    [Fact]
    public async Task Test_AuthorizeMatricula_Transferir_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // 1. ARRANGE
        // Prepara o cenário de banco padrão
        var (estudante, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();

        var novaTurma = DataFactory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId).Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
        }

        // 🎯 O PULO DO GATO: Define que a requisição de transferência será feita por um Admin
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // 2. ACT
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);

        // 3. ASSERT
        // O foco aqui é garantir que o segurança da API DEIXOU PASSAR
        response.StatusCode.Should().Be(HttpStatusCode.OK); // ou HttpStatusCode.NoContent dependendo da sua API

        // Mantém a sua validação de banco original para garantir que a regra de negócio rodou!
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var matriculaAntigaNoBanco = await contexto.Matriculas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == matriculaOriginal.Id);
            matriculaAntigaNoBanco!.Ativo.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Test_AuthorizeMatricula_Transferir_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // 1. ARRANGE
        var (estudante, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();

        var novaTurma = DataFactory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId).Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante");

        // 2. ACT
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);

        // 3. ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var matriculaAntigaNoBanco = await contexto.Matriculas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == matriculaOriginal.Id);
        }
    }
}