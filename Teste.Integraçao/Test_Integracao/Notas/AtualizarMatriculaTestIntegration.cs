using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Integracao.Notas;

[Collection("ApiMatrix")]
public class AtualizarMatriculaTestIntegration : IntegrationTestBase
{
    public AtualizarMatriculaTestIntegration(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    // Dentro da classe de teste ou de uma classe auxiliar (ex: TestDataBuilder)
    private async Task<(Matricula matricula, NotaDtoCreate novaNota)> PrepararCenarioDeNota(double valor = 9.5)
    {
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        var novaNota = Data_Factory.NotafakerDto.Clone().RuleFor(n => n.Valor, valor).Generate();

        return (matricula, novaNota);
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await Data_Factory.CriarCenarioDeMatriculaValido(contexto);
    }

    private NotaDtoUpdate GerarNotaAtualizada(double valor = 8.0) => Data_Factory.Notafakerup.Clone().RuleFor(n => n.Valor, valor).Generate();

    [Fact]
    public async Task Atualizar_com_Sucesso()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();

        var notaAtualizada = GerarNotaAtualizada();
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada.Id}", notaAtualizada);
        resposta.EnsureSuccessStatusCode();

        var notaObtida = await resposta.Content.ReadFromJsonAsync<NotaDtoResponse>();

        Assert.NotNull(notaObtida);
        Assert.Equal(notaCriada.Id, notaObtida.Id);
    }

    [Fact]
    public async Task Atualizar_Nota_NaoEncontrada()
    {
        var (matricula, _) = await PrepararCenarioDeNota();
        var notaAtualizada = GerarNotaAtualizada();
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{Guid.NewGuid()}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Nota_MatriculaIncorreta()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();
        var notaAtualizada = GerarNotaAtualizada();
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{Guid.NewGuid()}/notas/{notaCriada.Id}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Nota_ValorInvalido()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();
        var notaAtualizada = GerarNotaAtualizada(15.0); // Valor inválido
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada.Id}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Nota_ValorNegativo()
    {
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var RespostaCriacao = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        RespostaCriacao.EnsureSuccessStatusCode();
        var notaCriada = await RespostaCriacao.Content.ReadFromJsonAsync<NotaDtoResponse>();
        var notaAtualizada = GerarNotaAtualizada(-5.0); // Valor inválido
        var resposta = await _client.PutAsJsonAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada.Id}", notaAtualizada);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}