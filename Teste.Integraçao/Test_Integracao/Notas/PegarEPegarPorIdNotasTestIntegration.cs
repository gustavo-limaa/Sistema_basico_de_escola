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
public class PegarEPegarPorIdNotasTestIntegration : IntegrationTestBase
{
    public PegarEPegarPorIdNotasTestIntegration(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(Matricula matricula, NotaDtoCreate novaNota)> PrepararCenarioDeNota(double valor = 9.5)
    {
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        var novaNota = Data_Factory.NotafakerDto.Generate();

        return (matricula, novaNota);
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await Data_Factory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task PegarTodasNotas_DeveRetornarListaVazia()
    {
        var (matricula, _) = await PrepararCenarioDeNota();
        var response = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas");
        Assert.NotNull(response);
    }

    [Fact]
    public async Task PegarNotaPorId_DeveRetornarNota_QuandoNotaExistir()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota();

        var postResponse = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        postResponse.EnsureSuccessStatusCode();
        var notaCriada = await postResponse.Content.ReadFromJsonAsync<NotaDtoResponse>();
        // Act
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas/{notaCriada.Id}");
        getResponse.EnsureSuccessStatusCode();
        var notaObtida = await getResponse.Content.ReadFromJsonAsync<NotaDtoResponse>();
        // Assert
        Assert.NotNull(notaObtida);
        Assert.Equal(notaCriada.Id, notaObtida.Id);
        Assert.Equal(novaNota.Valor, notaObtida.Valor);
        Assert.Equal(novaNota.Descricao, notaObtida.Descricao);
        Assert.Equal(novaNota.Importancia, notaObtida.Importancia);
        Assert.Equal(novaNota.Categoria, notaObtida.Categoria);
    }

    [Fact]
    public async Task PegarNotaPorId_DeveRetornarNotFound_QuandoNotaNaoExistir()
    {
        // Arrange
        var (matricula, _) = await PrepararCenarioDeNota();
        var notaIdInexistente = Guid.NewGuid();
        // Act
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas/{notaIdInexistente}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task PegarTodasNotas_DeveRetornarListaComNotas()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var postResponse = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        postResponse.EnsureSuccessStatusCode();
        var notaCriada = await postResponse.Content.ReadFromJsonAsync<NotaDtoResponse>();
        // Act
        var getResponse = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas");
        getResponse.EnsureSuccessStatusCode();
        var notasObtidas = await getResponse.Content.ReadFromJsonAsync<List<NotaDtoResponse>>();
        // Assert
        Assert.NotNull(notasObtidas);
        var notaObtida = notasObtidas.FirstOrDefault(n => n.Id == notaCriada.Id);
        Assert.Equal(notaCriada.Id, notaObtida.Id);
        Assert.Equal(novaNota.Valor, notaObtida.Valor);
        Assert.Equal(novaNota.Descricao, notaObtida.Descricao);
        Assert.Equal(novaNota.Importancia, notaObtida.Importancia);
        Assert.Equal(novaNota.Categoria, notaObtida.Categoria);
    }

    [Fact]
    public async Task PegarNotaPorId_DeveRetornarNotFound_QuandoMatriculaNaoExistir()
    {
        // Arrange
        var matriculaIdInexistente = Guid.NewGuid();
        var notaIdInexistente = Guid.NewGuid();
        // Act
        var getResponse = await _client.GetAsync($"/api/matriculas/{matriculaIdInexistente}/notas/{notaIdInexistente}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task PegarTodasNotas_DeveRetornarNotFound_QuandoMatriculaNaoExistir()
    {
        var matricula = Data_Factory.MatriculaFaker(Guid.NewGuid(), Guid.NewGuid()).Generate();

        var response = await _client.GetAsync($"/api/matriculas/{matricula.Id}/notas");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PegarNotaPorId_DeveRetornarNotFound_QuandoNotaNaoPertencerAMatricula()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota();
        var postResponse = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        postResponse.EnsureSuccessStatusCode();
        var notaCriada = await postResponse.Content.ReadFromJsonAsync<NotaDtoResponse>();

        var outraMatriculaId = Guid.NewGuid(); // Matricula que não existe
        // Act
        var getResponse = await _client.GetAsync($"/api/matriculas/{outraMatriculaId}/notas/{notaCriada.Id}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}