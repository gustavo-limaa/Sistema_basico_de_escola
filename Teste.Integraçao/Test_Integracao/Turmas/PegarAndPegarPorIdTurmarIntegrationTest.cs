using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Turmas;

[Collection("ApiMatrix")]
public class PegarAndPegarPorIdTurmarIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public PegarAndPegarPorIdTurmarIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<EstudanteDtoResponse> CriarEstudanteAsync()
    {
        var fake = Data_Factory.EstudanteFakerup.Generate();
        var resp = await _client.PostAsJsonAsync("/api/estudante", fake);

        // Se falhar aqui, o xUnit vai te mostrar o Status Code real (ex: 404 ou 500)
        // em vez de dar erro de JSON.
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
    }

    private TurmaDtoCreate CriarTUrma() => Data_Factory.TurmaFakerdto(Guid.NewGuid(), Guid.NewGuid(), 30);

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
    public async Task Pegar_Turmas_RetornaListaDeTurmas()
    {
        // Arrange
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();
        var dadosFaker = CriarTUrma();

        var turmaParaCriar = new TurmaDtoCreate(
        discId, profId,
        dadosFaker.CapacidadeMaxima,
        dadosFaker.Sigla,
        dadosFaker.Semestre, dadosFaker.AnoLetivo,
        dadosFaker.Numero);

        var respPost = await _client.PostAsJsonAsync("/api/turmas", turmaParaCriar);
        respPost.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync("/api/turmas");

        // Assert
        response.EnsureSuccessStatusCode();
        var turmas = await response.Content.ReadFromJsonAsync<List<TurmaDtoResponse>>();

        turmas.Should().NotBeEmpty();
        turmas.Should().ContainSingle(t => t.NomeDisciplina == nomeDisc);
    }

    [Fact]
    public async Task Pegar_Turmas_Quando_Nao_Ha_Nenhuma_DeveRetornarListaVazia()
    {
        // Arrange: banco limpo (reset acontece no DisposeAsync/InitializeAsync da base)

        // Act
        var response = await _client.GetAsync("/api/turmas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var turmas = await response.Content.ReadFromJsonAsync<List<TurmaDtoResponse>>();

        turmas.Should().NotBeNull();
        turmas.Should().BeEmpty();
    }

    [Fact]
    public async Task Pegar_Turma_PorId_RetornaSucesso()
    {
        // Arrange
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();
        var dtoCriar = Data_Factory.TurmaFakerdto(profId, discId, 12).Generate();
        var respPost = await _client.PostAsJsonAsync("/api/turmas", dtoCriar);
        var turmaCriada = await respPost.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        // Act
        var response = await _client.GetAsync($"/api/turmas/{turmaCriada!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<TurmaDtoResponse>();
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(turmaCriada.Id);
        resultado.NomeDisciplina.Should().Be(nomeDisc);
    }

    [Fact]
    public async Task Pegar_Turma_PorId_Inexistente_DeveRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/turmas/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Pegar_Turma_PorId_Com_Formato_Invalido_DeveRetornarBadRequest()
    {
        // Act: manda um valor que não é um Guid válido na rota
        var response = await _client.GetAsync($"/api/turmas/dasdweada");

        // Assert: o model binding do ASP.NET Core rejeita antes de chegar no usecase
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}