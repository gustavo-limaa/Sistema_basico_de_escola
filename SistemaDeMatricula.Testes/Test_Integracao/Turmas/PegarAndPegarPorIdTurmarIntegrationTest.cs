using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Turmas;

[Collection("ApiMatrix")]
public class PegarAndPegarPorIdTurmarIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public PegarAndPegarPorIdTurmarIntegrationTest(SistemaMatriculaFactory factory)
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

        // 1. Limpa as Turmas ignorando qualquer filtro (pega as inativas também!)
        await contexto.Turmas
            .IgnoreQueryFilters()
            .ExecuteDeleteAsync();

        // 2. Agora sim, os pais podem ser removidos com segurança
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
    public async Task Pegar_Turmas_RetornaListaDeTurmas()
    {
        // Arrange
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();
        var dadosFaker = CriarTUrma();

        var turmaParaCriar = new TurmaDtoCreate(
        discId, profId, dadosFaker.Sigla,
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
    public async Task Deve_Rejeitar_Criacao_Com_Dados_Invalidos()
    {
        // Arrange
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();

        var dtocreat = new TurmaDtoCreate(
             DisciplinaId: discId,
             ProfessorId: profId,
             Sigla: "",
             Semestre: -1, // Inválido!
             AnoLetivo: 1999,
             Numero: 02131
        );

        // Act
        var respPost = await _client.PostAsJsonAsync("/api/turmas", dtocreat);

        // Assert
        // Aqui usamos o Fluent Assertions no objeto da resposta do POST
        respPost.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarTurma_ComProfessorInexistente_DeveRetornarBadRequest()
    {
        // 1. Arrange: Criamos uma Disciplina REAL (para garantir que o erro não seja nela)
        var respDisc = await _client.PostAsJsonAsync("/api/disciplinas", new DisciplinaDtoCreate("C# Avançado", 80));
        respDisc.EnsureSuccessStatusCode();
        var discDados = await respDisc.Content.ReadFromJsonAsync<DisciplinaDtoResponse>();

        var dadosFaker = CriarTUrma(); // Para pegar Sigla, Ano, etc.

        // 2. Criamos o DTO: Disciplina existe, mas o Professor é um ID aleatório
        var dto = new TurmaDtoCreate(
            DisciplinaId: discDados.DisciplinaId, // ✅ Existe
            ProfessorId: Guid.NewGuid(),          // ❌ Não existe
            Sigla: dadosFaker.Sigla,
            Semestre: dadosFaker.Semestre,
            AnoLetivo: dadosFaker.AnoLetivo,
            Numero: dadosFaker.Numero
        );

        // 3. Act
        var resposta = await _client.PostAsJsonAsync("/api/turmas", dto);

        // 4. Assert
        // Como o seu UseCase retorna Result.Falha, o status deve ser 400 (Bad Request)
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Opcional: Validar se a mensagem de erro é a que você escreveu no UseCase
        var conteudo = await resposta.Content.ReadAsStringAsync();
        conteudo.Should().Contain("Professor não encontrado.");
    }

    [Fact]
    public async Task CriarTurma_ComDisciplinaInexistente_DeveRetornarBadRequest()
    {
        // 1. Arrange: Criamos uma Disciplina REAL (para garantir que o erro não seja nela)
        var prof = CriarDtoValido();
        var resProf = await _client.PostAsJsonAsync("/api/professores", prof);

        resProf.EnsureSuccessStatusCode();
        var profDados = await resProf.Content.ReadFromJsonAsync<ProfessorDtoResponse>();

        var dadosFaker = CriarTUrma(); // Para pegar Sigla, Ano, etc.

        // 2. Criamos o DTO: Disciplina existe, mas o Professor é um ID aleatório
        var dto = new TurmaDtoCreate(
            DisciplinaId: Guid.NewGuid(), // ✅ Existe
            ProfessorId: profDados.ProfessorId,          // ❌ Não existe
            Sigla: dadosFaker.Sigla,
            Semestre: dadosFaker.Semestre,
            AnoLetivo: dadosFaker.AnoLetivo,
            Numero: dadosFaker.Numero
        );

        // 3. Act
        var resposta = await _client.PostAsJsonAsync("/api/turmas", dto);

        // 4. Assert
        // Como o seu UseCase retorna Result.Falha, o status deve ser 400 (Bad Request)
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Opcional: Validar se a mensagem de erro é a que você escreveu no UseCase
        var conteudo = await resposta.Content.ReadAsStringAsync();
        conteudo.Should().Contain("Disciplina não encontrada.");
    }

    [Fact]
    public async Task Pegar_Turma_PorId_RetornaSucesso()
    {
        // Arrange
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();
        var dadosFaker = CriarTUrma();
        var dtoCriar = new TurmaDtoCreate(discId, profId, dadosFaker.Sigla,
                                          dadosFaker.Semestre, dadosFaker.AnoLetivo,
                                          dadosFaker.Numero);

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
    public async Task Verificar_Se_O_Filtro_SoftDelete_Funciona()
    {
        // 1. Arrange: Cria a turma
        var (profId, discId, nomeDisc) = await CriarDependenciasAsync();
        var dadosFaker = CriarTUrma();
        var dtoCriar = new TurmaDtoCreate(discId, profId, dadosFaker.Sigla,
                                          dadosFaker.Semestre, dadosFaker.AnoLetivo,
                                          dadosFaker.Numero);

        var respPost = await _client.PostAsJsonAsync("/api/turmas", dtoCriar);
        var turmaCriada = await respPost.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        // 2. Act: Deleta a turma (Soft Delete)
        var respDelete = await _client.DeleteAsync($"/api/turmas/{turmaCriada!.Id}");
        respDelete.EnsureSuccessStatusCode();

        // 3. Assert Parte A: A API comum não deve mais retornar ela
        var respGetGeral = await _client.GetAsync("/api/turmas");
        var listaTurmas = await respGetGeral.Content.ReadFromJsonAsync<List<TurmaDtoResponse>>();
        listaTurmas.Should().NotContain(t => t.Id == turmaCriada.Id);

        // 4. Assert Parte B (O Pulo do Gato): Verificar direto no banco com IgnoreQueryFilters
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Usamos o .IgnoreQueryFilters() para conseguir ver o registro "Inativo"
        var turmaNoBanco = await contexto.Turmas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == turmaCriada.Id);

        turmaNoBanco.Should().NotBeNull();
        turmaNoBanco!.Ativo.Should().BeFalse(); // Prova que ela foi desativada, não apagada
    }
}