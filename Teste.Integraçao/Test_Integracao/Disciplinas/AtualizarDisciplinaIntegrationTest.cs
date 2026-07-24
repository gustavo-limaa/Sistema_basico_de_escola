using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Disciplinas;

[Collection("ApiMatrix")]
public class AtualizarDisciplinaIntegrationTest : IntegrationTestBase, IAsyncLifetime
{
    public AtualizarDisciplinaIntegrationTest(SistemaMatriculaFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AtualizarDisciplina_Sucesso()
    {
        // ARRANGE
        var dto = Data_Factory.DisciplinaFakerdto.Generate();
        var criarResponse = await _client.PostAsJsonAsync("/api/disciplinas", dto);
        criarResponse.EnsureSuccessStatusCode();
        var disciplinaCriada = await criarResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaCriada.Should().NotBeNull();

        var up = Data_Factory.DisciplinaFakerup.Generate();

        // ACT
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplinas/{disciplinaCriada!.DisciplinaId}", up);

        if (!atualizarResponse.IsSuccessStatusCode)
        {
            var mensagemErro = await atualizarResponse.Content.ReadAsStringAsync();
            throw new Exception($"A API retornou erro {atualizarResponse.StatusCode}: {mensagemErro}");
        }

        // ASSERT
        var disciplinaAtualizada = await atualizarResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaAtualizada.Should().NotBeNull();
        disciplinaAtualizada!.Nome.Should().Be(up.Nome);
        disciplinaAtualizada.CargaHoraria.Should().Be(up.CargaHoraria);
        disciplinaAtualizada.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task AtualizarDisciplina_NaoEncontrada()
    {
        // ARRANGE
        var up = Data_Factory.DisciplinaFakerup.Generate();

        // ACT
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplinas/{Guid.NewGuid()}", up);

        // ASSERT
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AtualizarDisciplina_NomeDuplicado()
    {
        // ARRANGE
        var dto1 = Data_Factory.DisciplinaFakerdto.Generate();
        var resultResponse1 = await _client.PostAsJsonAsync("/api/disciplinas", dto1);
        resultResponse1.EnsureSuccessStatusCode();
        var disciplinaCriada1 = await resultResponse1.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaCriada1.Should().NotBeNull();

        var dto2 = Data_Factory.DisciplinaFakerdto.Generate();
        var resultResponse2 = await _client.PostAsJsonAsync("/api/disciplinas", dto2);
        resultResponse2.EnsureSuccessStatusCode();
        var disciplinaCriada2 = await resultResponse2.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaCriada2.Should().NotBeNull();

        var upBase = Data_Factory.DisciplinaFakerup.Generate();
        var up = upBase with { Nome = disciplinaCriada1!.Nome };

        // ACT
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplinas/{disciplinaCriada2!.DisciplinaId}", up);

        // ASSERT
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarDisciplina_DadosInvalidos()
    {
        // ARRANGE
        var dto = Data_Factory.DisciplinaFakerdto.Generate();
        var resultResponse = await _client.PostAsJsonAsync("/api/disciplinas", dto);
        resultResponse.EnsureSuccessStatusCode();
        var disciplinaCriada = await resultResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaCriada.Should().NotBeNull();

        var upBase = Data_Factory.DisciplinaFakerup.Generate();
        var up = upBase with { Nome = string.Empty, CargaHoraria = -1 };

        // ACT
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplinas/{disciplinaCriada!.DisciplinaId}", up);

        // ASSERT
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarDisciplina_SemAlteracoes()
    {
        // ARRANGE
        var dto = Data_Factory.DisciplinaFakerdto.Generate();
        var resultResponse = await _client.PostAsJsonAsync("/api/disciplinas", dto);
        resultResponse.EnsureSuccessStatusCode();
        var disciplinaCriada = await resultResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaCriada.Should().NotBeNull();

        // Reenvia exatamente os mesmos dados da disciplina recém-criada
        var up = new DisciplinaDtoUpdate(
            disciplinaCriada!.DisciplinaId,
            disciplinaCriada.Nome,
            disciplinaCriada.CargaHoraria,
            disciplinaCriada.Ativo
        );

        // ACT
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/disciplinas/{disciplinaCriada.DisciplinaId}", up);
        atualizarResponse.EnsureSuccessStatusCode();

        // ASSERT
        var disciplinaAtualizada = await atualizarResponse.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        disciplinaAtualizada.Should().NotBeNull();
        disciplinaAtualizada!.Nome.Should().Be(disciplinaCriada.Nome);
        disciplinaAtualizada.CargaHoraria.Should().Be(disciplinaCriada.CargaHoraria);
        disciplinaAtualizada.Ativo.Should().Be(disciplinaCriada.Ativo);
    }
}