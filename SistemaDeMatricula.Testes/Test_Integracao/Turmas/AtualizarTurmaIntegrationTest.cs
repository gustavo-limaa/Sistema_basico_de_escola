using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using System.Net;
using System.Net.Http.Json;

namespace SistemaDeMatricula.Testes.Test_Integracao.Turmas;

[Collection("ApiMatrix")]
public class AtualizarTurmaIntegrationTest
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public AtualizarTurmaIntegrationTest(SistemaMatriculaFactory factory)
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

    private async Task<TurmaDtoResponse> CriarTurmaAsync(Guid profid, Guid discid) // 1. Nome padronizado e retorno correto
    {
        // Gera os dados fake
        var dto = DataFactory.TurmaFaker().Generate();

        var turmaDto = new TurmaDtoCreate
        (
            DisciplinaId: discid,
            ProfessorId: profid,
            Sigla: dto.CodigoTurma.Sigla,
            Semestre: dto.CodigoTurma.Semestre,
            AnoLetivo: dto.CodigoTurma.Ano,
            Numero: dto.CodigoTurma.Numero
        );

        // 2. Executa a criação real na API
        var resposta = await _client.PostAsJsonAsync("/api/turmas", turmaDto);

        if (!resposta.IsSuccessStatusCode)
        {
            var erroMsg = await resposta.Content.ReadAsStringAsync();
            throw new Exception($"Erro ao criar turma no setup: {erroMsg}");
        }
        // 3. Lê o objeto único de resposta (que contém o ID gerado pelo banco)
        var response = await resposta.Content.ReadFromJsonAsync<TurmaDtoResponse>();

        return response!;
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
    public async Task Deve_Atualizar_Turma_Com_Sucesso()
    {
        // 1. Arrange: Cria as dependências reais
        var (profId, discId, _) = await CriarDependenciasAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var profNoBanco = await db.Professores.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == profId);

        Console.WriteLine($"Professor no Banco: {profNoBanco.NomeCompleto} | Ativo: {profNoBanco.Ativo}");
        // 2. Criar a turma passando os IDs que acabamos de gerar
        // (Ajuste o método CriarTurmaAsync para aceitar esses parâmetros)
        var turmaCriada = await CriarTurmaAsync(profId, discId);
        var idDaTurma = turmaCriada.Id;

        // 3. Preparar os novos dados para o Update
        var dadosParaAtualizar = new TurmaDtoUpdate(
            ProfessorId: profId,
            DisciplinaId: discId,
            Ativo: true,
            Sigla: "HIS",
            Semestre: 2,
            AnoLetivo: 2027,
            Numero: 005
        );

        // 4. Act: Agora sim, passando o ID na URL e os dados no corpo
        var response = await _client.PutAsJsonAsync($"/api/turmas/{idDaTurma}", dadosParaAtualizar);
        response.EnsureSuccessStatusCode();
        // 5. Assert: Verificar se deu bom
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Dica extra: Verifique se os dados mudaram de verdade buscando a turma novamente
        var turmaAtualizada = await response.Content.ReadFromJsonAsync<TurmaDtoResponse>();
    }

    [Fact]
    public async Task deve_retornar_badrequest_quanto_dados_inalido()
    { // 1. Arrange: Cria as dependências reais
        var (profId, discId, _) = await CriarDependenciasAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var profNoBanco = await db.Professores.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == profId);

        Console.WriteLine($"Professor no Banco: {profNoBanco.NomeCompleto} | Ativo: {profNoBanco.Ativo}");
        // 2. Criar a turma passando os IDs que acabamos de gerar
        // (Ajuste o método CriarTurmaAsync para aceitar esses parâmetros)
        var turmaCriada = await CriarTurmaAsync(profId, discId);
        var idDaTurma = turmaCriada.Id;

        // 3. Preparar os novos dados para o Update
        var dadosParaAtualizar = new TurmaDtoUpdate(
            ProfessorId: profId,
            DisciplinaId: discId,
            Ativo: true,
            Sigla: "HIS",
            Semestre: -2,
            AnoLetivo: 2027,
            Numero: 005
        );

        // 4. Act: Agora sim, passando o ID na URL e os dados no corpo
        var response = await _client.PutAsJsonAsync($"/api/turmas/{idDaTurma}", dadosParaAtualizar);

        // 5. Assert: Verificar se deu bom
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);// Exemplo de como "abrir a caixa" do erro
        var erro = await response.Content.ReadAsStringAsync();
        erro.Should().Contain("Semestre"); // Garante que a falha foi onde você queria
    }

    [Fact]
    public async Task deve_retorna_idinvalido_quando_passarid_inescistente()
    {
        var response = await _client.DeleteAsync($"/api/turmas/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}