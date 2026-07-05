using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain.Uteis;
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
    private const string RoleAdmin = "Admin";

    private const string RoleEstudante = "Estudante";

    public Test_AuthorizeMatricula(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", role);
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await DataFactory.CriarCenarioDeMatriculaValido(contexto);
    }

    private async Task<EstudanteEntity> CriarNovoEstudanteNoBanco()
    {
        var novoEstudante = DataFactory.EstudanteFaker.Generate();
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexto.Estudantes.AddAsync(novoEstudante);
        await contexto.SaveChangesAsync();
        return novoEstudante;
    }

    [Fact]
    public async Task Criar_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        SetRole(RoleAdmin);
        var (_, turma, _) = await PrepararDadosNoBanco(); // Ignora o estudante que já vem matriculado
        var novoEstudante = await CriarNovoEstudanteNoBanco();
        var dto = new MatriculaDtoCreate(novoEstudante.Id, turma.Id);

        // ACT
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        // ASSERT
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK); // Ou 201 Created
    }

    [Fact]
    public async Task Criar_Matricula_Sem_Autorizacao_Deve_SER_Falha()
    {
        // ARRANGE
        SetRole(RoleEstudante);
        var (_, turma, _) = await PrepararDadosNoBanco();
        var novoEstudante = await CriarNovoEstudanteNoBanco();
        var dto = new MatriculaDtoCreate(novoEstudante.Id, turma.Id);

        // ACT
        var postResponse = await _client.PostAsJsonAsync("/api/matriculas", dto);

        // ASSERT
        postResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Obterporid_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        SetRole(RoleAdmin);
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Obterporid_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        // ARRANGE
        SetRole(RoleEstudante);
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Obtertodos_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        SetRole(RoleAdmin);
        await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _client.GetAsync("/api/matriculas/");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Obtertodos_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        // ARRANGE
        SetRole(RoleEstudante);
        await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _client.GetAsync("/api/matriculas/");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        SetRole(RoleAdmin);
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var deleteResponse = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        // ARRANGE
        SetRole(RoleEstudante);
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var deleteResponse = await _client.DeleteAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Transferir_Matricula_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();

        var novaTurma = DataFactory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId).Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
        }

        SetRole(RoleAdmin);

        // ACT
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK); // ou HttpStatusCode.NoContent dependendo da API

        // Confirma que a regra de negócio (desativar matrícula antiga) realmente rodou
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var matriculaAntigaNoBanco = await contexto.Matriculas.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == matriculaOriginal.Id);
            matriculaAntigaNoBanco!.Ativo.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Transferir_Matricula_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // ARRANGE
        var (_, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();

        var novaTurma = DataFactory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId).Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
        }

        SetRole(RoleEstudante);

        // ACT
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Confirma que a matrícula original NÃO foi alterada, já que a operação foi barrada
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var matriculaAntigaNoBanco = await contexto.Matriculas.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == matriculaOriginal.Id);
            matriculaAntigaNoBanco!.Ativo.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Criar_Nota_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        SetRole(RoleAdmin);
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);

        // ACT
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Criar_Nota_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        SetRole(RoleEstudante);
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);

        // ACT
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Listar_Notas_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        SetRole(RoleAdmin);
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        // ACT
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Listar_Notas_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        SetRole(RoleAdmin);
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "estudante");
        // ACT
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Obterporid_Nota_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        SetRole(RoleAdmin);
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        var post = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);
        var notaCriada = await post.Content.ReadFromJsonAsync<NotaDtoResponse>();
        notaCriada.Should().NotBeNull();

        // ACT
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada!.Id}");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Atualizar_Nota_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        SetRole(RoleAdmin);
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        var post = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);
        var notaCriada = await post.Content.ReadFromJsonAsync<NotaDtoResponse>();
        notaCriada.Should().NotBeNull();

        var notaAtualizada = new NotaDtoUpdate(9.5, "Atualizado", TipoImportancia.Media, CategoriaAvaliacao.Seminario);

        // ACT
        var putResponse = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada!.Id}", notaAtualizada);

        // ASSERT
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Atualizar_Nota_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        SetRole(RoleAdmin);
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        var post = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);
        var notaCriada = await post.Content.ReadFromJsonAsync<NotaDtoResponse>();
        notaCriada.Should().NotBeNull();

        SetRole(RoleEstudante);
        var notaAtualizada = new NotaDtoUpdate(9.5, "Atualizado", TipoImportancia.Media, CategoriaAvaliacao.Seminario);

        // ACT
        var putResponse = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada!.Id}", notaAtualizada);

        // ASSERT
        putResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}