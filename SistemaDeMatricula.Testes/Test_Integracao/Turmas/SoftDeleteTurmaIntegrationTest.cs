using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Turmas;

[Collection("ApiMatrix")]
public class SoftDeleteTurmaIntegrationTest
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public SoftDeleteTurmaIntegrationTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // 1. ANTES DE CADA TESTE: Não precisamos de nada especial aqui
    public Task InitializeAsync() => Task.CompletedTask;

    // 2. DEPOIS DE CADA TESTE: Aqui é onde a mágica da limpeza acontece
    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. O "neto" primeiro: Matrículas são as primeiras a sair
        // Use o IgnoreQueryFilters aqui também para garantir que nada escape
        await contexto.Matriculas.IgnoreQueryFilters().ExecuteDeleteAsync();

        // 2. O "filho": Agora as Turmas podem ir embora com segurança
        await contexto.Turmas.IgnoreQueryFilters().ExecuteDeleteAsync();

        // 3. Os "pais": Por fim, limpamos as entidades base
        await contexto.Professores.ExecuteDeleteAsync();
        await contexto.Disciplinas.ExecuteDeleteAsync();
        await contexto.Estudantes.ExecuteDeleteAsync();
    }

    private async Task<EstudanteDtoResponse> CriarEstudanteAsync()
    {
        var estu = DataFactory.EstudanteFaker.Generate();
        var fake = new EstudanteDtoCreate(
            NomeCompleto: estu.NomeCompleto.Valor,
            Email: estu.Email.Valor,
            DataNascimento: estu.DataNascimento.Valor,
            Cpf: estu.Cpf.Valor,
            Telefone: estu.Telefone.Valor

            );
        var resp = await _client.PostAsJsonAsync("/api/estudante", fake);

        // Se falhar aqui, o xUnit vai te mostrar o Status Code real (ex: 404 ou 500)
        // em vez de dar erro de JSON.
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<EstudanteDtoResponse>();
    }

    private ProfessorDtoCreate CriarDtoValido()
    {
        var professor = DataFactory.ProfessorFaker.Generate();
        var dto = new ProfessorDtoCreate(

           NomeCompleto: professor.NomeCompleto.Valor,
           Cpf: professor.Cpf.Valor,
           DataNascimento: professor.DataNascimento.Valor,
           Email: professor.Email.Valor,
           Telefone: professor.Telefone.Valor,
           Salario: professor.Salario.Valor,
           Categoria: professor.Categoria.ToString()
         );
        return dto;
    }

    private DisciplinaDtoCreate CriarDisciplina()
    {
        var disciplina = DataFactory.DisciplinaFaker.Generate();
        var dto = new DisciplinaDtoCreate(
           Nome: disciplina.Nome.Valor,
           CargaHoraria: disciplina.CargaHoraria.Valor
         );
        return dto;
    }

    private TurmaDtoCreate CriarTUrma()
    {
        var dto = DataFactory.TurmaFaker().Generate();

        var turmaDto = new TurmaDtoCreate
        (
            DisciplinaId: dto.DisciplinaId,
            ProfessorId: dto.ProfessorId,
            Sigla: dto.CodigoTurma.Sigla,
            Semestre: dto.CodigoTurma.Semestre,
            AnoLetivo: dto.CodigoTurma.Ano,
            Numero: dto.CodigoTurma.Numero
        );

        return turmaDto;
    }

    private async Task<(Guid ProfessorId, Guid DisciplinaId, string NomeDisciplina)> CriarDependenciasAsync()
    {
        // 1. Criar Professor
        var respProf = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var prof = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Criar Disciplina
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", CriarDisciplina());
        respDisc.EnsureSuccessStatusCode();
        var disc = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        return (prof!.ProfessorId, disc!.DisciplinaId, disc.Nome);
    }

    [Fact]
    public async Task NaoDevePermitir_SoftDelete_Quando_TurmaTemAlunos()
    {
        // 1. Arrange: Criar a Turma
        var (profId, discId, _) = await CriarDependenciasAsync();
        var dadosTurma = CriarTUrma();
        var dtoTurma = new TurmaDtoCreate(discId, profId, dadosTurma.Sigla,
                                          dadosTurma.Semestre, dadosTurma.AnoLetivo,
                                          dadosTurma.Numero);

        var respTurma = await _client.PostAsJsonAsync("/api/turmas", dtoTurma);
        var turmaCriada = await respTurma.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        // 2. Arrange: Criar um Estudante e Matricular (Simulando o fluxo)
        // Nota: Aqui usei nomes genéricos, adapte para seus DTOs de Estudante/Matricula
        var estudante = await CriarEstudanteAsync();

        var dtoMatricula = new { EstudanteId = estudante.EstudanteId, TurmaId = turmaCriada!.Id };
        // No seu Fact dentro do SoftDeleteTurmaIntegrationTest
        var respMatricula = await _client.PostAsJsonAsync("/api/matriculas", dtoMatricula);

        await _client.PostAsJsonAsync("/api/matriculas", dtoMatricula);

        // 3. Act: Tentar deletar a turma que agora tem "dono"
        var response = await _client.DeleteAsync($"/api/turmas/{turmaCriada.Id}");

        // 4. Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var erro = await response.Content.ReadAsStringAsync();
        erro.Should().Contain("Não é possível desativar uma turma com alunos matriculados.");
    }

    [Fact]
    public async Task deve_softdelete_com_susesso()
    {
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();

        var dadosturmas = CriarTUrma();

        var dadosvalidos = new TurmaDtoCreate(
            DisciplinaId: discId,
            ProfessorId: profId,
            Sigla: dadosturmas.Sigla,
            Semestre: dadosturmas.Semestre,
            AnoLetivo: dadosturmas.AnoLetivo,
            Numero: dadosturmas.Numero
        );
        var respTurma = await _client.PostAsJsonAsync("/api/turmas", dadosvalidos);

        var turmaCriada = await respTurma.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var response = await _client.DeleteAsync($"/api/turmas/{turmaCriada.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deve_dar_BadRequest_quando_Id_Invalido()
    {
        var response = await _client.DeleteAsync($"/api/turmas/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deve_Retornar_NoContent_Ao_Tentar_Desativar_Turma_Ja_Inativa()
    {
        // 1. Setup: Criar a turma e suas dependências
        var (profId, discId, _) = await CriarDependenciasAsync();
        var dadosBase = CriarTUrma();

        var dtoCreate = new TurmaDtoCreate(
            DisciplinaId: discId,
            ProfessorId: profId,
            Sigla: dadosBase.Sigla,
            Semestre: dadosBase.Semestre,
            AnoLetivo: dadosBase.AnoLetivo,
            Numero: dadosBase.Numero
        );

        var respCriar = await _client.PostAsJsonAsync("/api/turmas", dtoCreate);
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