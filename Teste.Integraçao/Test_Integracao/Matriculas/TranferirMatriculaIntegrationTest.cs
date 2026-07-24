using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Json;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Test_Integracao.Matriculas;

[Collection("ApiMatrix")]
public class TranferirMatriculaIntegrationTest : IntegrationTestBase
{
    public TranferirMatriculaIntegrationTest(SistemaMatriculaFactory factory) : base(factory)
    {
    }

    private async Task<(EstudanteEntity estudante, TurmaEntity turma, MatriculaEntity matricula)> PrepararDadosNoBanco()
    {
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await Data_Factory.CriarCenarioDeMatriculaValido(contexto);
    }

    [Fact]
    public async Task Traferir_Matricula_Com_Sucesso()
    {
        // 1. Preparar os dados no banco
        var (estudante, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();

        // 2. Criar uma nova turma para onde vamos transferir
        // (Passamos o id do professor da turma antiga ou deixamos gerar um novo para não quebrar a FK)
        var novaTurma = Data_Factory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId, 10).Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
        }

        // 3. Chamar a API de transferência (PATCH e por ID na URL)
        // O corpo leva apenas o GUID da nova turma direto, sem objeto anônimo!
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);
        response.EnsureSuccessStatusCode();

        // 4. Validar o resultado no banco
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // A matrícula original deve estar desativada
            var matriculaAntigaNoBanco = await contexto.Matriculas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == matriculaOriginal.Id);
            matriculaAntigaNoBanco.Should().NotBeNull();
            matriculaAntigaNoBanco!.Ativo.Should().BeFalse();

            // Deve existir uma nova matrícula ativa para a nova turma
            var novaMatriculaNoBanco = await contexto.Matriculas.AsNoTracking()
                .FirstOrDefaultAsync(m => m.EstudanteId == estudante.Id && m.TurmaId == novaTurma.Id && m.Ativo);
            novaMatriculaNoBanco.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Transferir_Matricula_Falha_Quando_Nova_Turma_Esta_Lotada()
    {
        // 1. Preparar os dados no banco
        var (estudante, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();
        // 2. Criar uma nova turma com capacidade 1 e já lotada
        var novaTurma = Data_Factory.TurmaFaker(turmaOriginal.ProfessorId, turmaOriginal.DisciplinaId, 1).Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await contexto.Turmas.AddAsync(novaTurma);
            await contexto.SaveChangesAsync();
            // Matricular um estudante fictício para lotar a turma
            var estudanteFicticio = Data_Factory.EstudanteFaker.Generate();
            estudanteFicticio.ativar();
            await contexto.Estudantes.AddAsync(estudanteFicticio);
            await contexto.SaveChangesAsync();
            var matriculaFicticia = new MatriculaEntity(estudanteFicticio.Id, novaTurma.Id);
            await contexto.Matriculas.AddAsync(matriculaFicticia);
            await contexto.SaveChangesAsync();
        }
        // 3. Chamar a API de transferência
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurma.Id);
        // 4. Validar que a resposta é de falha por turma lotada
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var errorMessage = await response.Content.ReadAsStringAsync();
        errorMessage.Should().Contain(MensagensTurma.TurmaLotada);
    }

    [Fact]
    public async Task Transferir_Matricula_Falha_Quando_Nova_Turma_Nao_Existe()
    {
        // 1. Preparar os dados no banco
        var (estudante, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();
        // 2. Gerar um GUID aleatório para a nova turma que não existe
        var novaTurmaIdInexistente = Guid.NewGuid();
        // 3. Chamar a API de transferência
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaOriginal.Id}/transferir", novaTurmaIdInexistente);
        // 4. Validar que a resposta é de falha por turma não existente
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var errorMessage = await response.Content.ReadAsStringAsync();
        errorMessage.Should().Contain(MensagensTurma.TurmaNaoEncontrada);
    }

    [Fact]
    public async Task Transferir_Matricula_Falha_Quando_Matricula_Original_Nao_Existe()
    {
        // 1. Preparar os dados no banco (só para garantir que temos uma turma válida)
        var (estudante, turmaOriginal, matriculaOriginal) = await PrepararDadosNoBanco();
        // 2. Gerar um GUID aleatório para a matrícula original que não existe
        var matriculaIdInexistente = Guid.NewGuid();
        // 3. Chamar a API de transferência
        var response = await _client.PatchAsJsonAsync($"/api/matriculas/{matriculaIdInexistente}/transferir", turmaOriginal.Id);
        // 4. Validar que a resposta é de falha por matrícula original não encontrada
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        var errorMessage = await response.Content.ReadAsStringAsync();
        errorMessage.Should().Contain(MensagensMatricula.MatriculaNaoEncontrada);
    }
}