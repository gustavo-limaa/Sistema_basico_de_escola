using FluentAssertions;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Turmas;

[Collection("ApiMatrix")]
public class CriarTurmaTestIntegration : IntegrationTestBase, IAsyncLifetime
{
    public CriarTurmaTestIntegration(SistemaMatriculaFactory factory)
        : base(factory)
    {
    }

    private ProfessorDtoCreate CriarDtoValido() => Data_Factory.ProfessorFakerdto.Generate();

    [Fact]
    public async Task CriarTurma_ComDadosValidos_DeveRetornar201EIdValido()
    {
        // 1. Criar dependências reais via API
        var respProf = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // 🎯 O PULO DO GATO: Passamos os IDs reais criados acima para o gerador de turma!
        var turmaValida = Data_Factory.TurmaFakerdto(profDados.ProfessorId, discDados.DisciplinaId, 12).Generate();

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaValida);

        // Assert
        if (!resposta.IsSuccessStatusCode)
        {
            var conteudoBruto = await resposta.Content.ReadAsStringAsync();
            throw new Exception($"A API REJEITOU: {resposta.StatusCode} - {conteudoBruto}");
        }

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        var respostaDados = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();
        respostaDados.Should().NotBeNull();
    }

    [Fact]
    public async Task CriarTurma_ComCodigoInvalido_DeveRetornar400()
    {
        // 1. Criar dependências reais via API
        var respProf = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // 2. Cria a turma vinculada aos IDs certos, mas avaria a Sigla com o "with"
        var turmaInvalida = Data_Factory.TurmaFakerdto(profDados.ProfessorId, discDados.DisciplinaId, 10).Generate();
        var conflito = turmaInvalida with { Sigla = "" };

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/turmas", conflito);

        // Assert
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarTurma_ComProfessorInativo_DeveRetornar400()
    {
        // 1. Criar Professor e Disciplina
        var respProf = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // 2. Desativar o Professor de verdade no sistema
        var respostaDesativacao = await _client.DeleteAsync($"/api/professores/{profDados.ProfessorId}");
        respostaDesativacao.EnsureSuccessStatusCode();

        // 3. Tenta criar a turma apontando para o ID do professor recém-desativado
        var turmaInvalida = Data_Factory.TurmaFakerdto(profDados.ProfessorId, discDados.DisciplinaId, 10).Generate();

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaInvalida);

        // Assert
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarTurma_ComDisciplinaInativa_DeveRetornar400()
    {
        // 1. Criar Professor e Disciplina
        var respProf = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // 2. Desativar a Disciplina
        var respostaDesativacao = await _client.DeleteAsync($"/api/disciplinas/{discDados.DisciplinaId}");
        respostaDesativacao.EnsureSuccessStatusCode();

        // 3. Monta a turma com a disciplina inativa
        var turmaInvalida = Data_Factory.TurmaFakerdto(profDados.ProfessorId, discDados.DisciplinaId, 10).Generate();

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaInvalida);

        // Assert
        // Se a regra diz que não pode criar com dependência inativada, deve retornar BadRequest (400)
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarTurma_ComCodigoDuplicado_DeveRetornar409()
    {
        // 1. Criar dependências
        var respProf = await _client.PostAsJsonAsync("/api/professores", CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", Data_Factory.DisciplinaFakerdto.Generate());
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // 2. Forçar uma turma válida amarrada aos IDs reais
        var turmaValida = Data_Factory.TurmaFakerdto(profDados.ProfessorId, discDados.DisciplinaId, 10).Generate();

        // Salva a primeira vez (Sucesso)
        var resposta1 = await _client.PostAsJsonAsync("/api/turmas", turmaValida);
        resposta1.EnsureSuccessStatusCode();

        // 3. Tenta criar EXATAMENTE a mesma turma novamente (mesmo código de sigla/ano/numero)
        // Act
        var resposta2 = await _client.PostAsJsonAsync("/api/turmas", turmaValida);

        // Assert
        resposta2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}