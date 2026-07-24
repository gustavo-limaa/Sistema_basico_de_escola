using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Integracao.Notas;

[Collection("ApiMatrix")]
public class AdicionarNotaIntegrationTests : IntegrationTestBase, IAsyncLifetime
{
    public AdicionarNotaIntegrationTests(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(Matricula matricula, NotaDtoCreate novaNota)> PrepararCenarioDeNota(double valor = 9.5)
    {
        var (estudante, turma, matricula) = await PrepararDadosNoBanco();

        var novaNota = Data_Factory.NotafakerDto.Clone().RuleFor(n => n.Valor, valor).Generate();
        ;
        return (matricula, novaNota);
    }

    private async Task<(EstudanteEntity, TurmaEntity, MatriculaEntity)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await Data_Factory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Sucesso()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(9.5);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);

        // Assert
        response.EnsureSuccessStatusCode();
        var notaCriada = await response.Content.ReadFromJsonAsync<NotaDtoResponse>();

        Assert.NotNull(notaCriada);
        Assert.Equal(novaNota.Valor, notaCriada.Valor);
    }

    [Fact]
    public async Task Adiciona_Notas_para_Matricula_Inexistente_Deve_Falhar()
    {
        // Arrange
        var novaNota = new NotaDtoCreate(
            Valor: 8.0,
            Descricao: "Boa participação",
            Importancia: TipoImportancia.Media
            ,
            Categoria: CategoriaAvaliacao.Prova
        );
        var matriculaIdInexistente = Guid.NewGuid(); // ID aleatório que não existe
        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matriculaIdInexistente}/notas", novaNota);
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Adiciona_Notas_para_Matricula_Inativa_Deve_Falhar()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(7.0);
        // Inativa a matrícula
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var matriculaDb = await contexto.Matriculas.FirstOrDefaultAsync(m => m.Id == matricula.Id);
        if (matriculaDb != null)
        {
            matriculaDb.desativar();
            await contexto.SaveChangesAsync();
        }
        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains(MensagensMatricula.MatriculaJaDesativada
            , errorMessage);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Valor_Zero_Deve_Ser_Sucesso()
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(0.0);
        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);
        // Assert
        response.EnsureSuccessStatusCode();
        var notaCriada = await response.Content.ReadFromJsonAsync<NotaDtoResponse>();
        Assert.NotNull(notaCriada);
        Assert.Equal(0.0, notaCriada.Valor);
    }

    [Theory]
    [InlineData(9.5)]
    [InlineData(0.0)]
    [InlineData(10.0)]
    public async Task Adiciona_Notas_com_Sucesso_Variados(double valor)
    {
        // Arrange
        var (matricula, novaNota) = await PrepararCenarioDeNota(valor);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", novaNota);

        // Assert
        response.EnsureSuccessStatusCode();
        var notaCriada = await response.Content.ReadFromJsonAsync<NotaDtoResponse>();
        Assert.Equal(valor, notaCriada.Valor);
    }

    [Fact]
    public async Task Adiciona_Notas_com_Valor_Negativo_Deve_Falhar_Variados()
    {
        // Arrange
        var (matricula, _) = await PrepararCenarioDeNota();
        var valoresNegativos = new[] { -0.01, -1.0, -100.0 };
        foreach (var valor in valoresNegativos)
        {
            var notaNegativa = new NotaDtoCreate(valor, "Valor negativo", TipoImportancia.Alta, CategoriaAvaliacao.FeiraDeCiencias);
            // Act
            var response = await _client.PostAsJsonAsync($"/api/matriculas/{matricula.Id}/notas", notaNegativa);
            var responseContent = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Nota", responseContent);
        }
    }
}