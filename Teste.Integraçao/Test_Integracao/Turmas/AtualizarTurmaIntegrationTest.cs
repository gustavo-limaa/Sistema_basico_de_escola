using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared; // 🚀 Puxando a Shared certa
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Turmas;

[Collection("ApiMatrix")]
public class AtualizarTurmaIntegrationTest : IntegrationTestBase
{
    public AtualizarTurmaIntegrationTest(SistemaMatriculaFactory factory)
        : base(factory)
    {
    }

    private async Task<EstudanteDtoResponse> CriarEstudanteAsync()
    {
        // 🎯 Usando o DTO direto do nosso gerador centralizado
        var fake = Data_Factory.EstudanteFakerdto.Generate();
        var resp = await _client.PostAsJsonAsync("/api/estudante", fake);
        resp.EnsureSuccessStatusCode();

        return (await resp.Content.ReadFromJsonAsync<EstudanteDtoResponse>())!;
    }

    private async Task<TurmaDtoResponse> CriarTurmaAsync(Guid profId, Guid discId, int capacidade = 30)
    {
        var turmaDto = Data_Factory.TurmaFakerdto(profId, discId, capacidade).Generate();

        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaDto);

        return (await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>())!;
    }

    private async Task<(Guid ProfessorId, Guid DisciplinaId, string NomeDisciplina)> CriarDependenciasAsync()
    {
        // 1. Criar Professor usando o DTO da nossa fábrica central
        var respProf = await _client.PostAsJsonAsync("/api/professores", Data_Factory.ProfessorFakerdto.Generate());
        respProf.EnsureSuccessStatusCode();
        var prof = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Criar Disciplina usando o DTO da nossa fábrica central
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var disc = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        return (prof!.ProfessorId, disc!.DisciplinaId, disc.Nome);
    }

    [Fact]
    public async Task Deve_Atualizar_Turma_Com_Sucesso()
    {
        // 1. Arrange: Cria as dependências reais
        var (profId, discId, _) = await CriarDependenciasAsync();
        var turmaCriada = await CriarTurmaAsync(profId, discId);
        var idDaTurma = turmaCriada.Id;

        // 2. Preparar os novos dados para o Update (Mudando capacidade e código)
        var dadosParaAtualizar = Data_Factory.TurmaFakerup(profId, discId, 30).Generate();

        // 3. Act: Passando o ID na URL e o payload no Put
        var response = await _client.PutAsJsonAsync($"/api/turmas/{idDaTurma}", dadosParaAtualizar);
        response.EnsureSuccessStatusCode();

        // 4. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var turmaAtualizada = await response.Content.ReadFromJsonAsync<TurmaDtoResponse>();
        turmaAtualizada.Should().NotBeNull();
        turmaAtualizada!.capacidadeMaxima.Should().Be(30);
    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Quando_Dados_Invalidos()
    {
        // 1. Arrange
        var (profId, discId, _) = await CriarDependenciasAsync();
        var turmaCriada = await CriarTurmaAsync(profId, discId);
        var idDaTurma = turmaCriada.Id;

        // 2. Injeta um Semestre inválido (-2) para estourar as validações de domínio/FluentValidation
        var dadosParaAtualizar = Data_Factory.TurmaFakerup(profId, discId, 12).Generate();

        var confllito = dadosParaAtualizar with { Semestre = -2 };
        // 3. Act
        var response = await _client.PutAsJsonAsync($"/api/turmas/{idDaTurma}", confllito);

        // 4. Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var erro = await response.Content.ReadAsStringAsync();
        erro.Should().Contain("Semestre");
    }

    [Fact]
    public async Task Deve_Retornar_NotFound_Quando_Passar_Id_Inexistente()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        // Usamos um DTO genérico para satisfazer o corpo da requisição do PUT
        var dadosParaAtualizar = Data_Factory.TurmaFakerup(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        // Act: 🎯 Corrigido para PUT em vez de DELETE para fazer sentido com o teste de atualização
        var response = await _client.PutAsJsonAsync($"/api/turmas/{Guid.NewGuid}", dadosParaAtualizar);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}