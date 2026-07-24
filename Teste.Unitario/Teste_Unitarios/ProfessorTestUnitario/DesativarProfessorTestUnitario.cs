using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Teste.Unit.Teste_Unitarios;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.ProfessorTestUnitario;

public class DesativarProfessorTestUnitario
{
    private readonly Mock<IUnitOfWork> _Uow;
    private readonly Mock<IRepositorioProfessor> _profe;

    private readonly ProfessorRemoverUsecase
        _caseDesativar;

    public DesativarProfessorTestUnitario()
    {
        _Uow = new Mock<IUnitOfWork>();
        _profe = new Mock<IRepositorioProfessor>();
        _caseDesativar = new ProfessorRemoverUsecase(_profe.Object);
    }

    [Fact]
    public async Task DesativarProfessor_Sucesso()
    {
        var professorId = Data_Factory.ProfessorFaker.Generate();

        professorId.ativar(); // Garante que o professor começa ativo
        _profe.Setup(p => p.ObterPorIdAsync(professorId.Id)).ReturnsAsync(professorId);

        var acao = await _caseDesativar.ExecutarAsync
            (professorId.Id);

        Assert.NotNull(acao);
    }

    [Fact]
    public async Task DesativarProfessor_ProfessorInexistente()
    {
        var professorId = Guid.NewGuid();
        _profe.Setup(p => p.ObterPorIdAsync(professorId)).ReturnsAsync((Professor)null);
        // ID que não existe
        var acao = await _caseDesativar.ExecutarAsync
            (professorId);
        acao.Mensagem.Should().Be(MensagensProfessor.ProfessorNaoEncontrado
            ); // Ajuste para a mensagem real do seu Use Case
    }

    [Fact]
    public async Task DesativarProfessor_ProfessorJaDesativado()
    {
        var professorId = Data_Factory.ProfessorFaker.Generate();
        professorId.desativar();
        _profe.Setup(p => p.ObterPorIdAsync(professorId.Id)).ReturnsAsync(professorId);

        var acao = await _caseDesativar.ExecutarAsync
            (professorId.Id);
        acao.Mensagem.Should().Be(MensagensProfessor.ErroInativo_ou_Ativo
            );
    }

    [Fact]
    public async Task DesativarProfessor_ProfessorComTurmasAtivas_DeveRetornarFalha()
    {
        // Arrange
        var professor = Data_Factory.ProfessorFaker.Generate();
        professor.ativar();

        _profe.Setup(p => p.ObterPorIdAsync(professor.Id)).ReturnsAsync(professor);

        // Ajustado para passar o .Id e o mock correto do repositório/uow
        _Uow.Setup(p => p.Professores.ExisteTurmaAtivaParaProfessorAsync(professor.Id)).ReturnsAsync(true);

        // Act
        var resultado = await _caseDesativar.ExecutarAsync(professor.Id);

        // Assert - Valida que o sistema BARRROU a desativação
        Assert.NotNull(resultado);
        Assert.False(resultado.Sucesso);
        Assert.Equal(MensagensProfessor.ErroSemAutoridade

            , resultado.Mensagem); // Ou a mensagem real que você colocou no Use Case
    }
}