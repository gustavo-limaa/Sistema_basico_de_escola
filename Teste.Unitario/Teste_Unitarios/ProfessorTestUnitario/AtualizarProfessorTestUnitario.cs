using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Services;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Teste.Unit.Teste_Unitarios;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.ProfessorTestUnitario;

public class AtualizarProfessorTestUnitario
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IRepositorioProfessor> _repositorioProfessor;
    private readonly ProfessorAtualizarUsecase _professor;
    private readonly Mock<IUsuarioLogadoService> _usuarioLogadoServiceMock;

    public AtualizarProfessorTestUnitario()
    {
        _uow = new Mock<IUnitOfWork>();
        _repositorioProfessor = new Mock<IRepositorioProfessor>();
        _usuarioLogadoServiceMock = new Mock<IUsuarioLogadoService>();
        _professor = new ProfessorAtualizarUsecase(_repositorioProfessor.Object, _usuarioLogadoServiceMock.Object);
    }

    [Fact]
    public async Task AtualizarProfessor_Deve_Atualizar_Professor_Cadastrado()
    {
        // ARRANGE
        var professor = Data_Factory.ProfessorFaker.Generate();

        // 🎯 O PULO DO GATO: Garantimos que o usuário logado é o mesmo dono do perfil do professor!
        var usuarioIdFake = professor.UsuarioId;

        _usuarioLogadoServiceMock.Setup(x => x.ObterUsuarioId()).Returns(usuarioIdFake);
        _usuarioLogadoServiceMock.Setup(x => x.Ehadmin()).Returns(false);

        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professor.Id))
            .ReturnsAsync(professor);

        var professorAtualizado = Data_Factory.ProfessorFakerup.Generate() with { ProfessorId = professor.Id };

        _repositorioProfessor.Setup(x => x.ObterPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Modelos.Professor?)null);

        _repositorioProfessor.Setup(x => x.SalvarAlteracoesAsync()).ReturnsAsync(true);

        // ACT
        var result = await _professor.ExecutarAsync(professorAtualizado);

        // ASSERT
        result.Sucesso.Should().BeTrue(because: result.Mensagem);
        _repositorioProfessor.Verify(x => x.Atualizar(professor), Times.Once);
    }

    [Fact]
    public async Task AtualizarProfessor_Deve_Retornar_Erro_Quando_Professor_Nao_Existir()
    {
        var professorId = Guid.NewGuid();
        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professorId)).ReturnsAsync((Professor)null);
        var professorAtualizado = Data_Factory.ProfessorFakerup.Generate();
        // Act
        var result = await _professor.ExecutarAsync(professorAtualizado);
        Assert.NotNull(result);
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be(MensagensProfessor.ProfessorNaoEncontrado);
    }

    [Fact]
    public async Task AtualizarProfessor_Deve_Retornar_Erro_Quando_Salvar_Alteracoes_Falhar()
    {
        var professor = Data_Factory.ProfessorFaker.Generate();
        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professor.Id)).ReturnsAsync(professor);
        var professorAtualizado = Data_Factory.ProfessorFakerup.Generate();
        _repositorioProfessor.Setup(x => x.Atualizar(professor)).Verifiable();
        _repositorioProfessor.Setup(x => x.SalvarAlteracoesAsync()).ReturnsAsync(false);
        // Act
        var result = await _professor.ExecutarAsync(professorAtualizado);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AtualizarProfessor_ComEmailJaExistente_DeveRetornarFalha()
    {
        // Arrange
        var professorEditando = Data_Factory.ProfessorFaker.Generate();
        var outroProfessor = Data_Factory.ProfessorFaker.Generate(); // ID diferente!

        var dtoUpdate = Data_Factory.ProfessorFakerup.Generate();

        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professorEditando.Id)).ReturnsAsync(professorEditando);
        _repositorioProfessor.Setup(x => x.ObterPorEmailAsync(dtoUpdate.Email)).ReturnsAsync(outroProfessor);

        // Act
        var result = await _professor.ExecutarAsync(dtoUpdate);

        // Assert
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be(MensagensProfessor.ProfessorNaoEncontrado);
    }

    [Fact]
    public async Task deve_retornar_erro_quando_dados_do_professor_sao_invalidos()
    {
        // Arrange
        ProfessorDtoUpdate professorDto = null;
        // Act
        var result = await _professor.ExecutarAsync(professorDto);
        // Assert
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be(MensagensProfessor.ProfessorInvalido);
    }

    [Fact]
    public async Task deve_retornar_erro_quando_professor_nao_for_encontrado()
    {
        // Arrange
        var professorId = Guid.NewGuid();
        var professorDto = Data_Factory.ProfessorFakerup.Generate();
        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professorId)).ReturnsAsync((Professor)null);
        // Act
        var result = await _professor.ExecutarAsync(professorDto);
        // Assert
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be(MensagensProfessor.ProfessorNaoEncontrado);
    }
}