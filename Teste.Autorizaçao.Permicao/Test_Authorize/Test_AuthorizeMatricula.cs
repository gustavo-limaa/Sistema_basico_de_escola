using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Authorize;

public class Test_AuthorizeMatricula : PermissaoTestBase
{
    public Test_AuthorizeMatricula(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        // 🎯 CORREÇÃO: Usando 'Factory' herdado da base
        using var scope = _Factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await Data_Factory.CriarCenarioDeMatriculaValido(contexto);
    }

    private async Task<EstudanteEntity> CriarNovoEstudanteNoBanco()
    {
        var novoEstudante = Data_Factory.EstudanteFaker.Generate();
        // 🎯 CORREÇÃO: Usando 'Factory' herdado da base
        using var scope = _Factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexto.Estudantes.AddAsync(novoEstudante);
        await contexto.SaveChangesAsync();
        return novoEstudante;
    }

    [Fact]
    public async Task Criar_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        ResetarParaAdmin();
        var (_, turma, _) = await PrepararDadosNoBanco();
        var novoEstudante = await CriarNovoEstudanteNoBanco();
        var dto = Data_Factory.MatriculaFaker(novoEstudante.Id, turma.Id).Generate();

        // ACT - 🎯 CORREÇÃO: Usando 'Client' herdado da base
        var postResponse = await _Client.PostAsJsonAsync("/api/matriculas", dto);

        // ASSERT
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Criar_Matricula_Sem_Autorizacao_Deve_SER_Falha()
    {
        // ARRANGE
        AutenticarComoEstudante();
        var (_, turma, _) = await PrepararDadosNoBanco();
        var novoEstudante = await CriarNovoEstudanteNoBanco();
        var dto = Data_Factory.MatriculaFaker(novoEstudante.Id, turma.Id).Generate();

        // ACT - 🎯 CORREÇÃO: Usando 'Client' herdado da base
        var postResponse = await _Client.PostAsJsonAsync("/api/matriculas", dto);

        // ASSERT
        postResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Obterporid_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        ResetarParaAdmin();
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _Client.GetAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Obterporid_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        // ARRANGE
        AutenticarComoEstudante();
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _Client.GetAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Obtertodos_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        ResetarParaAdmin();
        await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _Client.GetAsync("/api/matriculas/");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Obtertodos_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        // ARRANGE
        AutenticarComoEstudante();
        await PrepararDadosNoBanco();

        // ACT
        var getResponse = await _Client.GetAsync("/api/matriculas/");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Matricula_com_Autorizacao_Deve_SER_SUCESSO()
    {
        // ARRANGE
        ResetarParaAdmin();
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var deleteResponse = await _Client.DeleteAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Matricula_sem_Autorizacao_Deve_SER_falha()
    {
        // ARRANGE
        AutenticarComoEstudante();
        var (_, _, matricula) = await PrepararDadosNoBanco();

        // ACT
        var deleteResponse = await _Client.DeleteAsync($"/api/matriculas/{matricula.Id}");

        // ASSERT
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Transferir_Matricula_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();

        var novaTurma = Data_Factory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId, 10).Generate();
        using (var scope =
             _Factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
        }
        ResetarParaAdmin();

        // ACT
        var response = await _Client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK); // ou HttpStatusCode.NoContent dependendo da API

        // Confirma que a regra de negócio (desativar matrícula antiga) realmente rodou
        using (var scope = _Factory.Services.CreateScope())
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

        var novaTurma = Data_Factory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId, 10).Generate();
        using (var scope = _Factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
        }

        AutenticarComoEstudante();
        // ACT
        var response = await _Client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Confirma que a matrícula original NÃO foi alterada, já que a operação foi barrada
        using (var scope = _Factory.Services.CreateScope())
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
        ResetarParaAdmin();
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);

        // ACT
        var response = await _Client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Criar_Nota_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        AutenticarComoEstudante();
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);

        // ACT
        var response = await _Client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Listar_Notas_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        ResetarParaAdmin();
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        await _Client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        // ACT
        var getResponse = await _Client.GetAsync($"/api/matriculas/{matricula.Id}/notas");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Listar_Notas_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        ResetarParaAdmin();
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        await _Client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);

        _Client.DefaultRequestHeaders.Remove("X-Test-Role");
        _Client.DefaultRequestHeaders.Add("X-Test-Role", "estudante");
        // ACT
        var getResponse = await _Client.GetAsync($"/api/matriculas/{matricula.Id}/notas");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Obterporid_Nota_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        ResetarParaAdmin();
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        var post = await _Client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);
        var notaCriada = await post.Content.ReadFromJsonAsync<NotaDtoResponse>();
        notaCriada.Should().NotBeNull();

        // ACT
        var getResponse = await _Client.GetAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada!.Id}");

        // ASSERT
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Atualizar_Nota_DeveSerSucesso_QuandoUsuarioForAdmin()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        ResetarParaAdmin();
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        var post = await _Client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);
        var notaCriada = await post.Content.ReadFromJsonAsync<NotaDtoResponse>();
        notaCriada.Should().NotBeNull();

        var notaAtualizada = new NotaDtoUpdate(9.5, "Atualizado", TipoImportancia.Media, CategoriaAvaliacao.Seminario);

        // ACT
        var putResponse = await _Client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada!.Id}", notaAtualizada);

        // ASSERT
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Atualizar_Nota_DeveSerFalha_QuandoUsuarioForEstudante()
    {
        // ARRANGE
        var (_, _, matricula) = await PrepararDadosNoBanco();
        ResetarParaAdmin();
        var notaDtoCreate = new NotaDtoCreate(9.5, "Excelente desempenho", TipoImportancia.Alta, CategoriaAvaliacao.Apresentacao);
        var post = await _Client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaDtoCreate);
        var notaCriada = await post.Content.ReadFromJsonAsync<NotaDtoResponse>();
        notaCriada.Should().NotBeNull();

        AutenticarComoEstudante();
        var notaAtualizada = new NotaDtoUpdate(9.5, "Atualizado", TipoImportancia.Media, CategoriaAvaliacao.Seminario);

        // ACT
        var putResponse = await _Client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada!.Id}", notaAtualizada);

        // ASSERT
        putResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}