using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain.Modelos;
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
public class CriarTurmaTestIntegration : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public CriarTurmaTestIntegration(SistemaMatriculaFactory factory)
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

        // 1. Limpa as Turmas (Filho)
        await contexto.Turmas.ExecuteDeleteAsync();

        // 2. Limpa os Professores e Disciplinas (Pais)
        // Se você tiver Matrículas, elas devem ser limpas ANTES das Turmas.
        await contexto.Professores.ExecuteDeleteAsync();
        await contexto.Disciplinas.ExecuteDeleteAsync();
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

    private TurmaDtoCreate CriarTUrma()
    {
        var dto = DataFactory.TurmaFaker().Generate();

        return new TurmaDtoCreate
        (
            DisciplinaId: dto.DisciplinaId,
            ProfessorId: dto.ProfessorId,
            Sigla: dto.CodigoTurma.Sigla,
            Semestre: dto.CodigoTurma.Semestre,
            AnoLetivo: dto.CodigoTurma.Ano,
            Numero: dto.CodigoTurma.Numero
        );
    }

    [Fact]
    public async Task CriarTurma_ComDadosValidos_DeveRetornar201EIdValido()
    {
        // 1. Criar e persistir as dependências primeiro
        // 1. Criar o Professor e pegar o ID real// 1. Criar o Professor
        var respProf = await _client.PostAsJsonAsync("/api/professores",
            CriarDtoValido());

        if (!respProf.IsSuccessStatusCode)
        {
            var erroProf = await respProf.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao criar Professor: {respProf.StatusCode} - {erroProf}");
        }
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        // 2. Criar a Disciplina e pegar o ID real
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas",
            new DisciplinaDtoCreate("C# Avançado", 80));
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        // 3. Agora sim, criar a Turma usando IDs que EXISTEM no banco
        var turmaValida = new TurmaDtoCreate(
    discDados.DisciplinaId,
    profDados.ProfessorId,
    "CSH",
    1, // <--- Aqui está o erro! Você passou o ANO no lugar do SEMESTRE.
    2026,    // <--- Aqui você passou o 1 no lugar do ANO.
    001
);
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaValida);

        // PEGADINHA: Se deu erro, vamos ler como string para ver a "bronca" do servidor
        if (!resposta.IsSuccessStatusCode)
        {
            var conteudoBruto = await resposta.Content.ReadAsStringAsync();
            // Esse throw vai fazer o erro aparecer no Test Explorer detalhadamente
            throw new Exception($"A API REJEITOU: {resposta.StatusCode} - {conteudoBruto}");
        }

        // Se passou do IF, aí sim tentamos ler o JSON
        var respostaDados = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();
        var erro = respostaDados is null ? "Resposta nula" : "Resposta lida com sucesso";
    }

    [Fact]
    public async Task CriarTurma_ComCodigoInvalido_DeveRetornar400()
    {
        // 1. Criar e persistir as dependências primeiro
        var respProf = await _client.PostAsJsonAsync("/api/professores",
            CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas",
            new DisciplinaDtoCreate("C# Avançado", 80));
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // 2. Criar a Turma com código inválido (ex: semestre 3)
        var turmaInvalida = new TurmaDtoCreate(
            discDados.DisciplinaId,
            profDados.ProfessorId,
            "CSH",
            -1, // SEMESTRE INVÁLIDO
            2026,
            001
        );
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaInvalida);
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarTurma_ComProfessorInativo_DeveRetornar400()
    {
        // 1. Criar e persistir as dependências primeiro
        var respProf = await _client.PostAsJsonAsync("/api/professores",
            CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        // 2. Criar a Disciplina e pegar o ID real
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas",
            new DisciplinaDtoCreate("C# Avançado", 80));
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // 3. Desativar o Professor
        var respostaDesativacao = await _client.DeleteAsync($"/api/professores/{profDados.ProfessorId}");
        respostaDesativacao.EnsureSuccessStatusCode();
        // 4. Tentar criar a Turma com o Professor inativo
        var turmaInvalida = new TurmaDtoCreate(
            discDados.DisciplinaId,
            profDados.ProfessorId, // Professor inativo
            "CSH",
            1,
            2026,
            001
        );
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaInvalida);
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarTurma_ComDisciplinaInativa_DeveRetornar400()
    {
        // 1. Criar e persistir as dependências primeiro
        var respProf = await _client.PostAsJsonAsync("/api/professores",
            CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        // 2. Criar a Disciplina e pegar o ID real
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas",
            new DisciplinaDtoCreate("C# Avançado", 80));
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // 3. Desativar a Disciplina
        var respostaDesativacao = await _client.DeleteAsync($"/api/disciplinas/{discDados.DisciplinaId}");
        respostaDesativacao.EnsureSuccessStatusCode();
        // 4. Tentar criar a Turma com a Disciplina inativa
        var turmaInvalida = new TurmaDtoCreate(
            discDados.DisciplinaId, // Disciplina inativa
            profDados.ProfessorId,
            "CSH",
            1,
            2026,
            001
        );
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaInvalida);
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarTurma_ComCodigoDuplicado_DeveRetornar409()
    {
        // 1. Criar e persistir as dependências primeiro
        var respProf = await _client.PostAsJsonAsync("/api/professores",
            CriarDtoValido());
        respProf.EnsureSuccessStatusCode();
        var profDados = await respProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas",
            new DisciplinaDtoCreate("C# Avançado", 80));
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();
        // 2. Criar a Turma pela primeira vez (deve dar certo)
        var turmaValida = new TurmaDtoCreate(
            discDados.DisciplinaId,
            profDados.ProfessorId,
            "CSH",
            1,
            2026,
            001
        );
        var resposta1 = await _client.PostAsJsonAsync("/api/turmas", turmaValida);
        resposta1.EnsureSuccessStatusCode();
        // 3. Tentar criar a mesma Turma novamente (mesmo código)
        var resposta2 = await _client.PostAsJsonAsync("/api/turmas", turmaValida);
        resposta2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CriarTurma_SemDependencias_DeveRetornar400()
    {
        // 1. Tentar criar a Turma sem criar Professor e Disciplina antes
        var turmaInvalida = new TurmaDtoCreate(
            Guid.NewGuid(), // ID aleatório que não existe
            Guid.NewGuid(), // ID aleatório que não existe
            "CSH",
            1,
            2026,
            001
        );
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaInvalida);
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}