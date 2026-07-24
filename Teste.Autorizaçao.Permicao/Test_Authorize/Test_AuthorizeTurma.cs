using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Authorize;

public class Test_AuthorizeTurma : PermissaoTestBase
{
    public Test_AuthorizeTurma(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(Guid ProfessorId, Guid DisciplinaId)> CriarDependenciasAsync()
    {
        // 1. Criar Professor usando a fábrica limpa
        var respProf = await _Client.PostAsJsonAsync("/api/professores", Data_Factory.ProfessorFakerdto.Generate());
        respProf.EnsureSuccessStatusCode();
        var prof = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Criar Disciplina usando a fábrica limpa
        var respDisc = await _Client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var disc = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        return (prof!.ProfessorId, disc!.DisciplinaId);
    }

    [Fact]
    public async Task Test_Authorize_POST_Turma_Com_e_sem_Autorizacao()
    {
        // ==========================================
        // ADMIN (SUCESSO)
        // ==========================================
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();

        var turmaDtoAdmin = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();
        var respostaAdmin = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);

        respostaAdmin.StatusCode.Should().Be(HttpStatusCode.Created);
        var turmaCriada = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();
        turmaCriada.Should().NotBeNull();

        // ==========================================
        // ESTUDANTE (FALHA)
        // ==========================================
        ResetarParaAdmin();
        var (profIdEstudante, discIdEstudante) = await CriarDependenciasAsync();

        AutenticarComoEstudante();
        var turmaDtoEstudante = Data_Factory.TurmaFakerdto(profIdEstudante, discIdEstudante, 12).Generate();
        var respostaEstudante = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoEstudante);

        respostaEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_GETALL_Turma_Com_Autorizacao()
    {
        // Arrange
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaDto = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();
        await _Client.PostAsJsonAsync("/api/turmas", turmaDto);

        // Act & Assert
        var getAllResponseAdmin = await _Client.GetAsync("/api/turmas");
        getAllResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert
        AutenticarComoEstudante();
        var getAllResponseEstudante = await _Client.GetAsync("/api/turmas");
        getAllResponseEstudante.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Authorize_DELETE_Turma_Com_e_sem_Autorizacao()
    {
        // ==========================================
        // ADMIN (SUCESSO)
        // ==========================================
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaDtoAdmin = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();

        var respostaAdmin = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var deleteResponseAdmin = await _Client.DeleteAsync($"/api/turmas/{turmaCriadaAdmin!.Id}");
        deleteResponseAdmin.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ==========================================
        // ESTUDANTE (FALHA)
        // ==========================================
        ResetarParaAdmin();
        var (profIdt, discIdt) = await CriarDependenciasAsync();
        var turmaDtoEstudante = Data_Factory.TurmaFakerdto(profIdt, discIdt, 12).Generate();

        var respostaEstudante = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoEstudante);
        var turmaCriadaEstudante = await respostaEstudante.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        AutenticarComoEstudante(); // 🎯 O estudante tenta deletar
        var deleteResponseEstudante = await _Client.DeleteAsync($"/api/turmas/{turmaCriadaEstudante!.Id}");

        deleteResponseEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_PUT_Turma_Com_e_sem_Autorizacao()
    {
        // ==========================================
        // ADMIN (SUCESSO)
        // ==========================================
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaDtoAdmin = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();

        var respostaAdmin = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var dadosParaAtualizar = Data_Factory.TurmaFakerup(turmaDtoAdmin.ProfessorId, turmaDtoAdmin.DisciplinaId, 23).Generate();
        var putResponseAdmin = await _Client.PutAsJsonAsync($"/api/turmas/{turmaCriadaAdmin!.Id}", dadosParaAtualizar);
        putResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // ESTUDANTE (FALHA)
        // ==========================================
        ResetarParaAdmin();
        var (profIdr, discIdr) = await CriarDependenciasAsync();
        var turmaDto = Data_Factory.TurmaFakerdto(profIdr, discIdr, 12).Generate();

        var resposta = await _Client.PostAsJsonAsync("/api/turmas", turmaDto);
        var turmaCriada = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        AutenticarComoEstudante();
        var dadosParaAtualizarEstudante = Data_Factory.TurmaFakerup(turmaDto.ProfessorId, turmaDto.DisciplinaId, 12).Generate();
        var putEstudante = await _Client.PutAsJsonAsync($"/api/turmas/{turmaCriada!.Id}", dadosParaAtualizarEstudante);

        putEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_GET_TurmaById_Com_e_sem_Autorizacao()
    {
        // Arrange Admin
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaDtoAdmin = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();
        var respostaAdmin = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        // Act & Assert Admin
        var getByIdResponseAdmin = await _Client.GetAsync($"/api/turmas/{turmaCriadaAdmin!.Id}");
        getByIdResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        // Arrange & Act Estudante
        ResetarParaAdmin();
        var (profIdT, discIdR) = await CriarDependenciasAsync();
        var turmaDtoT = Data_Factory.TurmaFakerdto(profIdT, discIdR, 12).Generate();
        var resposta = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoT);
        var turmaCriada = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        AutenticarComoEstudante();
        var getByIdResponseEstudante = await _Client.GetAsync($"/api/turmas/{turmaCriada!.Id}");
        getByIdResponseEstudante.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Authorize_Restaurar_TurmaById_Com_e_sem_Autorizacao()
    {
        // ==========================================
        // ADMIN (SUCESSO)
        // ==========================================
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaDtoAdmin = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();
        var respostaAdmin = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        await _Client.DeleteAsync($"/api/turmas/{turmaCriadaAdmin!.Id}"); // Deleta primeiro

        var restaurarResponseAdmin = await _Client.PatchAsJsonAsync($"/api/turmas/{turmaCriadaAdmin!.Id}/restaurar", new { });
        restaurarResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // ESTUDANTE (FALHA)
        // ==========================================
        ResetarParaAdmin();
        var (profIdt, discIdt) = await CriarDependenciasAsync();
        var turmaDtoEstudante = Data_Factory.TurmaFakerdto(profIdt, discIdt, 12).Generate();

        var respostaEstudante = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoEstudante);
        var turmaCriadaEstudante = await respostaEstudante.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        await _Client.DeleteAsync($"/api/turmas/{turmaCriadaEstudante!.Id}");

        AutenticarComoEstudante(); // 🎯 O estudante tenta restaurar
        var restaurarResponseEstudante = await _Client.PatchAsJsonAsync($"/api/turmas/{turmaCriadaEstudante!.Id}/restaurar", new { });

        restaurarResponseEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}