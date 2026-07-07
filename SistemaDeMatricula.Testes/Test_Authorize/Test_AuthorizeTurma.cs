using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SistemaDeMatricula.Testes.Test_Authorize;

[Collection("ApiMatrix")]
public class Test_AuthorizeTurma : IntegrationTestBase, IAsyncLifetime
{
    private const string RoleAdmin = "Admin";

    private const string RoleEstudante = "Estudante";

    public Test_AuthorizeTurma(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", role);
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
    public async Task Test_Authorize_POST_Turma_Com_e_sem_Autorizacao()
    {
        SetRole(RoleAdmin);

        var (profId, discId, _) = await CriarDependenciasAsync();

        var dtoFake = DataFactory.TurmaFaker().Generate();
        var turmaDtoAdmin = new TurmaDtoCreate(discId, profId, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);
        var respostaAdmin = await _client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);

        respostaAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
        var turmaCriada = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();
        turmaCriada.Should().NotBeNull();

        SetRole(RoleAdmin);
        var (profIdParaEstudante, discIdParaEstudante, _) = await CriarDependenciasAsync();

        SetRole(RoleEstudante);

        var turmaDtoEstudante = new TurmaDtoCreate(discIdParaEstudante, profIdParaEstudante, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);

        var respostaEstudante = await _client.PostAsJsonAsync("/api/turmas", turmaDtoEstudante);

        respostaEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_GETALL_Turma_Com_Autorizacao()
    {
        SetRole(RoleAdmin);

        var (profId, discId, _) = await CriarDependenciasAsync();
        var dtoFake = DataFactory.TurmaFaker().Generate();
        var turmaDtoAdmin = new TurmaDtoCreate(discId, profId, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);
        var respostaAdmin = await _client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);

        var getAllResponseAdmin = await _client.GetAsync("/api/turmas");
        getAllResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        //ESTUDANTE

        SetRole(RoleAdmin);

        var (profIdT, discIdR, _) = await CriarDependenciasAsync();
        var dtoFakeR = DataFactory.TurmaFaker().Generate();
        var turmaDtoT = new TurmaDtoCreate(discId, profId, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);
        var resposta = await _client.GetAsync("/api/turmas");
        var turmaCriada = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        SetRole(RoleEstudante);
        var getAllResponseEstudante = await _client.GetAsync("/api/turmas");
        getAllResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Authorize_DELETE_Turma_Com_e_sem_Autorizacao()
    {
        // ==========================================
        // ADMIN (SUCESSO)
        // ==========================================
        SetRole(RoleAdmin);

        var (profId, discId, _) = await CriarDependenciasAsync();
        var dtoFake = DataFactory.TurmaFaker().Generate();
        var turmaDtoAdmin = new TurmaDtoCreate(discId, profId, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);
        var respostaAdmin = await _client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var DELETEResponseAdmin = await _client.DeleteAsync($"/api/turmas/{turmaCriadaAdmin!.Id}");
        DELETEResponseAdmin.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ==========================================
        // ESTUDANTE (FALHA)
        // ==========================================
        SetRole(RoleAdmin); // Mantém Admin para criar as dependências sem tomar 403

        var (profIdt, discIdt, _) = await CriarDependenciasAsync();
        var dtoFaker = DataFactory.TurmaFaker().Generate();
        var turmaDtoEstudante = new TurmaDtoCreate(discIdt, profIdt, dtoFaker.CapacidadeMaxima, dtoFaker.CodigoTurma.Sigla, dtoFaker.CodigoTurma.Semestre, dtoFaker.CodigoTurma.Ano, dtoFaker.CodigoTurma.Numero);

        var respostaEstudante = await _client.PostAsJsonAsync("/api/turmas", turmaDtoEstudante);
        // 🎯 CORREÇÃO AQUI: Lê o conteúdo da requisição certa ('respostaEstudante')
        var turmaCriadaEstudante = await respostaEstudante.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        // Ativa o vilão do teste
        SetRole(RoleEstudante);

        // 🎯 CORREÇÃO AQUI: O estudante tenta deletar a turma nova dele ('turmaCriadaEstudante')
        var DELETEResponseEstudante = await _client.DeleteAsync($"/api/turmas/{turmaCriadaEstudante!.Id}");
        DELETEResponseEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_PUT_Turma_Com_e_sem_Autorizacao()
    {
        SetRole(RoleAdmin);

        var (profId, discId, _) = await CriarDependenciasAsync();
        var dtoFake = DataFactory.TurmaFaker().Generate();
        var turmaDtoAdmin = new TurmaDtoCreate(discId, profId, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);
        var respostaAdmin = await _client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var dadosParaAtualizar = new TurmaDtoUpdate(
            ProfessorId: profId,
            DisciplinaId: discId,
            novaCapacidade: 123,
            Ativo: true,
            Sigla: "HIS",
            Semestre: 2,
            AnoLetivo: 2027,
            Numero: 005
        );

        var PUTResponseAdmin = await _client.PutAsJsonAsync($"/api/turmas/{turmaCriadaAdmin!.Id}", dadosParaAtualizar);
        PUTResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole(RoleAdmin); // Mantém Admin para criar as dependências sem tomar 403
        var (profIdr, discIdr, _) = await CriarDependenciasAsync();
        var dtoFaker = DataFactory.TurmaFaker().Generate();
        var turmaDto = new TurmaDtoCreate(discId, profId, dtoFaker.CapacidadeMaxima, dtoFaker.CodigoTurma.Sigla, dtoFaker.CodigoTurma.Semestre, dtoFaker.CodigoTurma.Ano, dtoFaker.CodigoTurma.Numero);
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaDto);
        var turmaCriada = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        SetRole(RoleEstudante);
        var dadosParaAtualizarEstudante = new TurmaDtoUpdate(
            ProfessorId: profIdr,
            DisciplinaId: discIdr,
            novaCapacidade: 456,
            Ativo: false,
            Sigla: "MAT",
            Semestre: 1,
            AnoLetivo: 2028,
            Numero: 010
        );
        var putestudante = await _client.PutAsJsonAsync($"/api/turmas/{turmaCriada!.Id}", dadosParaAtualizarEstudante);

        putestudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Authorize_GET_TurmaById_Com_e_sem_Autorizacao()
    {
        SetRole(RoleAdmin);

        var (profId, discId, _) = await CriarDependenciasAsync();
        var dtoFake = DataFactory.TurmaFaker().Generate();
        var turmaDtoAdmin = new TurmaDtoCreate(discId, profId, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);
        var respostaAdmin = await _client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var getbyIdResponseAdmin = await _client.GetAsync($"/api/turmas/{turmaCriadaAdmin.Id}");
        getbyIdResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        //ESTUDANTE

        SetRole(RoleAdmin);

        var (profIdT, discIdR, _) = await CriarDependenciasAsync();
        var dtoFakeR = DataFactory.TurmaFaker().Generate();
        var turmaDtoT = new TurmaDtoCreate(discIdR, profIdT, dtoFakeR.CapacidadeMaxima, dtoFakeR.CodigoTurma.Sigla, dtoFakeR.CodigoTurma.Semestre, dtoFakeR.CodigoTurma.Ano, dtoFakeR.CodigoTurma.Numero);
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaDtoT);
        var turmaCriada = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        SetRole(RoleEstudante);
        var getBYIDResponseEstudante = await _client.GetAsync($"/api/turmas/{turmaCriada.Id}");
        getBYIDResponseEstudante.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Authorize_Restaurar_TurmaById_Com_e_sem_Autorizacao()
    {
        SetRole(RoleAdmin);

        var (profId, discId, _) = await CriarDependenciasAsync();
        var dtoFake = DataFactory.TurmaFaker().Generate();
        var turmaDtoAdmin = new TurmaDtoCreate(discId, profId, dtoFake.CapacidadeMaxima, dtoFake.CodigoTurma.Sigla, dtoFake.CodigoTurma.Semestre, dtoFake.CodigoTurma.Ano, dtoFake.CodigoTurma.Numero);
        var respostaAdmin = await _client.PostAsJsonAsync("/api/turmas", turmaDtoAdmin);
        var turmaCriadaAdmin = await respostaAdmin.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var DELETEResponseAdmin = await _client.DeleteAsync($"/api/turmas/{turmaCriadaAdmin!.Id}");

        var restaurarResponseAdmin = await _client.PatchAsJsonAsync($"/api/turmas/{turmaCriadaAdmin!.Id}/restaurar", new { });
        var erroMensagem = await restaurarResponseAdmin.Content.ReadAsStringAsync();
        // Coloque um breakpoint aqui e inspecione a variável 'erroMensagem'
        restaurarResponseAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole(RoleAdmin); // Mantém Admin para criar as dependências sem tomar 403

        var (profIdt, discIdt, _) = await CriarDependenciasAsync();
        var dtoFaker = DataFactory.TurmaFaker().Generate();
        var turmaDtoEstudante = new TurmaDtoCreate(discIdt, profIdt, dtoFaker.CapacidadeMaxima, dtoFaker.CodigoTurma.Sigla, dtoFaker.CodigoTurma.Semestre, dtoFaker.CodigoTurma.Ano, dtoFaker.CodigoTurma.Numero);

        var respostaEstudante = await _client.PostAsJsonAsync("/api/turmas", turmaDtoEstudante);

        var turmaCriadaEstudante = await respostaEstudante.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        var DELETEResponseEstudante = await _client.DeleteAsync($"/api/turmas/{turmaCriadaEstudante!.Id}");

        // Ativa o vilão do teste
        SetRole(RoleEstudante);

        var restaurarResponseEstudante = await _client.PatchAsJsonAsync($"/api/turmas/{turmaCriadaEstudante!.Id}/restaurar", new { });

        restaurarResponseEstudante.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}