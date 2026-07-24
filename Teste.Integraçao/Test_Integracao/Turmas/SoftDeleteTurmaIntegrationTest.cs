using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Turmas;

[Collection("ApiMatrix")]
public class SoftDeleteTurmaIntegrationTest : IntegrationTestBase
{
    public SoftDeleteTurmaIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<EstudanteDtoResponse> CriarEstudanteAsync()
    {
        var fake = Data_Factory.EstudanteFakerdto.Generate();
        var resp = await _client.PostAsJsonAsync("/api/estudante", fake);

        // Se falhar aqui, o xUnit vai te mostrar o Status Code real (ex: 404 ou 500)
        // em vez de dar erro de JSON.
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
    }

    private TurmaDtoCreate CriarTUrma(Guid profId, Guid discId, int capacidade = 30)
    => Data_Factory.TurmaFakerdto(profId, discId, capacidade);

    private async Task<(Guid ProfessorId, Guid DisciplinaId, string NomeDisciplina)> CriarDependenciasAsync()
    {
        // 1. Criar Professor
        var respProf = await _client.PostAsJsonAsync("/api/professores", Data_Factory.ProfessorFakerdto.Generate());
        respProf.EnsureSuccessStatusCode();
        var prof = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Criar Disciplina
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var disc = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        return (prof!.ProfessorId, disc!.DisciplinaId, disc.Nome);
    }

    [Fact]
    public async Task NaoDevePermitir_SoftDelete_Quando_TurmaTemAlunos()
    {
        // 1. Arrange: Criar a Turma
        var (profId, discId, _) = await CriarDependenciasAsync();
        var dadosTurma = CriarTUrma(profId, discId);

        var respTurma = await _client.PostAsJsonAsync("/api/turmas", dadosTurma);
        var turmaCriada = await respTurma.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var estudante = await CriarEstudanteAsync();

        var dtoMatricula = new { EstudanteId = estudante.EstudanteId, TurmaId = turmaCriada!.Id };
        // No seu Fact dentro do SoftDeleteTurmaIntegrationTest
        var respMatricula = await _client.PostAsJsonAsync("/api/matriculas", dtoMatricula);

        await _client.PostAsJsonAsync("/api/matriculas", dtoMatricula);

        // 3. Act: Tentar deletar a turma que agora tem "dono"
        var response = await _client.DeleteAsync($"/api/turmas/{turmaCriada.Id}");

        // 4. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict
            );

        var erro = await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task deve_softdelete_com_susesso()
    {
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();

        var dadosturmas = CriarTUrma(profId, discId);

        var respTurma = await _client.PostAsJsonAsync("/api/turmas", dadosturmas);

        var turmaCriada = await respTurma.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var response = await _client.DeleteAsync($"/api/turmas/{turmaCriada.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deve_dar_notFound_quando_Id_Invalido()
    {
        var response = await _client.DeleteAsync($"/api/turmas/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deve_Retornar_NoContent_Ao_Tentar_Desativar_Turma_Ja_Inativa()
    {
        // 1. Setup: Criar a turma e suas dependências
        var (profId, discId, _) = await CriarDependenciasAsync();
        var dadosBase = CriarTUrma(profId, discId);

        var respCriar = await _client.PostAsJsonAsync("/api/turmas", dadosBase);
        respCriar.EnsureSuccessStatusCode();

        var turmaCriada = await respCriar.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        // 2. Primeira Desativação (Soft Delete real)
        await _client.DeleteAsync($"/api/turmas/{turmaCriada.Id}");

        // 3. Segunda Desativação (A tentativa redundante)
        var response = await _client.DeleteAsync($"/api/turmas/{turmaCriada.Id}");

        // 4. Assert: Deve continuar retornando 204 No Content
        // Isso prova que sua API é idempotente e não "explode" em erros
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}