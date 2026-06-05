using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.MatriculasTestUnitario;

public class TranferirMatriculaTestUnitario
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly TransferirEstudanteUsecase _useCase;

    public TranferirMatriculaTestUnitario()
    {
        _uowMock = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };
        _useCase = new TransferirEstudanteUsecase(_uowMock.Object);
    }

    [Fact]
    public async Task ExecutarAsync_Deve_Transferir_Matricula_Com_Sucesso()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        var novaTurmaId = Guid.NewGuid();
        var matriculaAntiga = new Matricula(matriculaId
            , novaTurmaId);

        var novaTurma = DataFactory.TurmaFaker().Generate();

        _uowMock.Setup(u => u.Matriculas.ObterPorIdAsync(matriculaId))
            .ReturnsAsync(matriculaAntiga);

        _uowMock.Setup(u => u.Turmas.ObterPorIdAsync(novaTurmaId))
            .ReturnsAsync(novaTurma);
        _uowMock.Setup(u => u.Matriculas.ContarMatriculasAtivasNaTurmaAsync(novaTurmaId))
            .ReturnsAsync(29);

        _uowMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(true);
        // Act
        var resultado = await _useCase.ExecutarAsync(matriculaId, novaTurmaId);
        // Assert
        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Dados);
        Assert.Equal(novaTurmaId, resultado.Dados.TurmaId);
    }

    [Fact]
    public async Task ExecutarAsync_Deve_Falhar_Quando_Nova_Turma_Estiver_Lotada()
    {
        var matriculaId = Guid.NewGuid();
        var novaTurmaId = Guid.NewGuid();
        var matriculaAntiga = new Matricula(Guid.NewGuid(), Guid.NewGuid());

        var novaTurma = DataFactory.TurmaFaker(capacidadeForçada: 30).Generate();

        _uowMock.Setup(u => u.Matriculas.ObterPorIdAsync(matriculaId))
            .ReturnsAsync(matriculaAntiga);
        _uowMock.Setup(u => u.Turmas.ObterPorIdAsync(novaTurmaId))
            .ReturnsAsync(novaTurma);
        _uowMock.Setup(u => u.Matriculas.ContarMatriculasAtivasNaTurmaAsync(novaTurmaId))
            .ReturnsAsync(30);
        // Act
        var resultado = await _useCase.ExecutarAsync(matriculaId, novaTurmaId);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("A nova turma já atingiu o limite de alunos.");

        _uowMock.Verify(u => u.Matriculas.AtualizarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_Deve_Falhar_Quando_Matricula_Original_Nao_Existir()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        var novaTurmaId = Guid.NewGuid();
        _uowMock.Setup(u => u.Matriculas.ObterPorIdAsync(matriculaId))
            .ReturnsAsync((Matricula)null);
        var resultado = await _useCase.ExecutarAsync(matriculaId, novaTurmaId);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("Matrícula original não encontrada.", resultado.Mensagem);
    }

    [Fact]
    public async Task ExecutarAsync_Deve_Falhar_Quando_Nova_Turma_Nao_Existir()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        var novaTurmaId = Guid.NewGuid();
        var matriculaAntiga = new Matricula(Guid.NewGuid(), Guid.NewGuid());
        _uowMock.Setup(u => u.Matriculas.ObterPorIdAsync(matriculaId))
            .ReturnsAsync(matriculaAntiga);
        _uowMock.Setup(u => u.Turmas.ObterPorIdAsync(novaTurmaId))
            .ReturnsAsync((Turma)null); // Simula nova turma não encontrada
                                        // Act
        var resultado = await _useCase.ExecutarAsync(matriculaId, novaTurmaId);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("A nova turma não existe.", resultado.Mensagem);
    }

    [Fact]
    public async Task ExecutarAsync_Deve_Falhar_Quando_Commit_Falhar()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        var novaTurmaId = Guid.NewGuid();
        var matriculaAntiga = new Matricula(Guid.NewGuid(), Guid.NewGuid());
        var novaTurma = DataFactory.TurmaFaker().Generate();
        _uowMock.Setup(u => u.Matriculas.ObterPorIdAsync(matriculaId))
            .ReturnsAsync(matriculaAntiga);
        _uowMock.Setup(u => u.Turmas.ObterPorIdAsync(novaTurmaId))
            .ReturnsAsync(novaTurma);
        _uowMock.Setup(u => u.Matriculas.ContarMatriculasAtivasNaTurmaAsync(novaTurmaId))
            .ReturnsAsync(29);
        _uowMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(false); // Simula falha no commit
        // Act
        var resultado = await _useCase.ExecutarAsync(matriculaId, novaTurmaId);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("Falha técnica ao processar a transferência no banco de dados.", resultado.Mensagem);
    }
}