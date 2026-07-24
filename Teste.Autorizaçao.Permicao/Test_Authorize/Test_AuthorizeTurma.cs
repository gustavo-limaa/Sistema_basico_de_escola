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
        var respProf = await _Client.PostAsJsonAsync("/api/professores", Data_Factory.ProfessorFakerdto.Generate());
        respProf.EnsureSuccessStatusCode();
        var prof = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var respDisc = await _Client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var disc = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        return (prof!.ProfessorId, disc!.DisciplinaId);
    }

    private async Task<TurmaDtoResponse> CriarTurmaValidaAsync(Guid profId, Guid discId)
    {
        var turmaDto = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();
        var resposta = await _Client.PostAsJsonAsync("/api/turmas", turmaDto);

        // 🎯 Se falhar no setup da turma, exibe a mensagem de erro detalhada da API
        if (!resposta.IsSuccessStatusCode)
        {
            var detalheErro = await resposta.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Erro ao preparar a Turma para o teste: {detalheErro}");
        }

        var turmaCriada = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();
        return turmaCriada!;
    }

    [Fact]
    public async Task Test_Authorize_POST_Turma_Com_e_sem_Autorizacao()
    {
        // ADMIN (SUCESSO)
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaDtoAdmin = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();

        var respostaAdmin = await _Client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        respostaAdmin.StatusCode.Should().Be(HttpStatusCode.Created);

        // ESTUDANTE (FALHA - FORBIDDEN)
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
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        await CriarTurmaValidaAsync(profId, discId);

        var getAllResponseAdmin = await _Client.GetAsync("/api/turmas");
        getAllResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        AutenticarComoEstudante();
        var getAllResponseEstudante = await _Client.GetAsync("/api/turmas");
        getAllResponseEstudante.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Authorize_DELETE_Turma_Com_e_sem_Autorizacao()
    {
        // ADMIN (SUCESSO)
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaCriadaAdmin = await CriarTurmaValidaAsync(profId, discId);

        var deleteResponseAdmin = await _Client.DeleteAsync($"/api/turmas/{turmaCriadaAdmin.Id}");
        deleteResponseAdmin.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ESTUDANTE (FALHA)
        ResetarParaAdmin();
        var (profIdt, discIdt) = await CriarDependenciasAsync();
        var turmaCriadaEstudante = await CriarTurmaValidaAsync(profIdt, discIdt);

        AutenticarComoEstudante();
        var deleteResponseEstudante = await _Client.DeleteAsync($"/api/turmas/{turmaCriadaEstudante.Id}");
        deleteResponseEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_PUT_Turma_Com_e_sem_Autorizacao()
    {
        // ADMIN (SUCESSO)
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaCriadaAdmin = await CriarTurmaValidaAsync(profId, discId);

        var dadosParaAtualizar = Data_Factory.TurmaFakerup(profId, discId, 23).Generate();
        var putResponseAdmin = await _Client.PutAsJsonAsync($"/api/turmas/{turmaCriadaAdmin.Id}", dadosParaAtualizar);
        putResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
        if (!putResponseAdmin.IsSuccessStatusCode)
        {
            var detalhe500 = await putResponseAdmin.Content.ReadAsStringAsync();
            throw new Exception($"[ERRO 500 NO PUT]: {detalhe500}");
        }
        // ESTUDANTE (FALHA)
        ResetarParaAdmin();
        var (profIdr, discIdr) = await CriarDependenciasAsync();
        var turmaCriada = await CriarTurmaValidaAsync(profIdr, discIdr);

        AutenticarComoEstudante();
        var dadosParaAtualizarEstudante = Data_Factory.TurmaFakerup(profIdr, discIdr, 12).Generate();
        var putEstudante = await _Client.PutAsJsonAsync($"/api/turmas/{turmaCriada.Id}", dadosParaAtualizarEstudante);
        putEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_GET_TurmaById_Com_e_sem_Autorizacao()
    {
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaCriadaAdmin = await CriarTurmaValidaAsync(profId, discId);

        var getByIdResponseAdmin = await _Client.GetAsync($"/api/turmas/{turmaCriadaAdmin.Id}");
        getByIdResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        ResetarParaAdmin();
        var (profIdT, discIdR) = await CriarDependenciasAsync();
        var turmaCriada = await CriarTurmaValidaAsync(profIdT, discIdR);

        AutenticarComoEstudante();
        var getByIdResponseEstudante = await _Client.GetAsync($"/api/turmas/{turmaCriada.Id}");
        getByIdResponseEstudante.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Authorize_Restaurar_TurmaById_Com_e_sem_Autorizacao()
    {
        // ADMIN (SUCESSO)
        ResetarParaAdmin();
        var (profId, discId) = await CriarDependenciasAsync();
        var turmaCriadaAdmin = await CriarTurmaValidaAsync(profId, discId);

        await _Client.DeleteAsync($"/api/turmas/{turmaCriadaAdmin.Id}");

        var restaurarResponseAdmin = await _Client.PatchAsJsonAsync($"/api/turmas/{turmaCriadaAdmin.Id}/restaurar", new { });
        restaurarResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        // ESTUDANTE (FALHA)
        ResetarParaAdmin();
        var (profIdt, discIdt) = await CriarDependenciasAsync();
        var turmaCriadaEstudante = await CriarTurmaValidaAsync(profIdt, discIdt);

        await _Client.DeleteAsync($"/api/turmas/{turmaCriadaEstudante.Id}");

        AutenticarComoEstudante();
        var restaurarResponseEstudante = await _Client.PatchAsJsonAsync($"/api/turmas/{turmaCriadaEstudante.Id}/restaurar", new { });
        restaurarResponseEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}