using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.MatriculasTestUnitario;

public class RemoverMatriculaTestUnitario
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly DesativarMatriculaUsecase _casoRemover;

    public RemoverMatriculaTestUnitario()
    {
        _uowMock = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };
        _casoRemover = new DesativarMatriculaUsecase(_uowMock.Object);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Desativar_Matricula_Com_Sucesso()
    {
        // 1. Geramos o ID que vai ser usado no teste todo
        var matriculaId = Guid.NewGuid();

        var matriculaFake = new Matricula(matriculaId, Guid.NewGuid());

        _uowMock.Setup(r => r.Matriculas.ObterPorIdAsync(matriculaId))
                .ReturnsAsync(matriculaFake);

        _uowMock.Setup(r => r.CommitAsync())
                .ReturnsAsync(true);

        // ACT
        var resultado = await _casoRemover.ExecutarAsync(matriculaId);

        // ASSERT
        resultado.Sucesso.Should().BeTrue();
        resultado.Dados.Should().BeTrue();

        _uowMock.Verify(r => r.Matriculas.AtualizarAsync(matriculaFake), Times.Once);
        matriculaFake.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Tentar_Desativar_Matricula_Ja_Desativada()
    {
        var matriculaId = Guid.NewGuid();
        var matriculaFake = new Matricula(matriculaId, Guid.NewGuid());
        matriculaFake.Desativar();
        _uowMock.Setup(r => r.Matriculas.ObterPorIdAsync(matriculaId))
                .ReturnsAsync(matriculaFake);
        // ACT
        var resultado = await _casoRemover.ExecutarAsync(matriculaId);
        // ASSERT
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensMatricula.MatriculaJaDesativada
            );
        _uowMock.Verify(r => r.Matriculas.AtualizarAsync(It.IsAny<Matricula>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Tentar_Desativar_Matricula_Inexistente()
    {
        var matriculaId = Guid.NewGuid();
        _uowMock.Setup(r => r.Matriculas.ObterPorIdAsync(matriculaId))
                .ReturnsAsync((Matricula)null);
        // ACT
        var resultado = await _casoRemover.ExecutarAsync(matriculaId);
        // ASSERT
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensMatricula.MatriculaNaoEncontrada);
        _uowMock.Verify(r => r.Matriculas.AtualizarAsync(It.IsAny<Matricula>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Tentar_Desativar_Matricula_Com_Id_Vazio()
    {
        //arrange
        var matriculaId = Guid.Empty;
        // ACT
        var resultado = await _casoRemover.ExecutarAsync(matriculaId);
        // ASSERT
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensMatricula.MatriculaNaoEncontrada);

        _uowMock.Verify(r => r.Matriculas.ObterPorIdAsync(It.IsAny<Guid>()), Times.Never);
        _uowMock.Verify(r => r.Matriculas.AtualizarAsync(It.IsAny<Matricula>()), Times.Never);
    }
}