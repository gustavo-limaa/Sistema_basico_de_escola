using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Test.Shared;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.EstudanteTestUnitario;

public class AtualizarEstudanteUseCaseTest
{
    private readonly Mock<IRepositorioEstudante> _repositorioMock;
    private readonly UsesCasesAtualizarEstudante _useCase;

    public AtualizarEstudanteUseCaseTest()
    {
        _repositorioMock = new Mock<IRepositorioEstudante>();
        _useCase = new UsesCasesAtualizarEstudante(_repositorioMock.Object);
    }

    [Fact]
    public async Task Deve_Atualizar_Estudante_Corretamente()
    {
        // Arrange
        var estudante = Data_Factory.EstudanteFaker.Generate();
        var dtoAtualizacao = Data_Factory.EstudanteFakerup.Generate();
        _repositorioMock.Setup(r => r.ObterPorIdAsync(estudante.Id))
                        .ReturnsAsync(estudante);
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(true);
        // Act
        var resultado = await _useCase.ExecuteAsync(estudante.Id, dtoAtualizacao);
        // Assert
        Assert.True(resultado.Sucesso);
        _repositorioMock.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Estudante_Nao_Encontrado()
    {
        // Arrange
        var estudante = Data_Factory.EstudanteFaker.Generate();
        var dtoAtualizacao = Data_Factory.EstudanteFakerup.Generate();

        _repositorioMock.Setup(r => r.ObterPorIdAsync(estudante.Id))
                        .ReturnsAsync(estudante);
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(true);
        // Act
        var resultado = await _useCase.ExecuteAsync(Guid.NewGuid(), dtoAtualizacao);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal(MensagensEstudante.ErroEstudanteNaoEncontrado, resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Houver_Erro_Ao_Atualizar()
    {
        // Arrange
        var estudante = Data_Factory.EstudanteFaker.Generate();

        var dtoAtualizacao = Data_Factory.EstudanteFakerup.Generate();

        _repositorioMock.Setup(r => r.ObterPorIdAsync(estudante.Id))
                        .ReturnsAsync(estudante);

        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ThrowsAsync(new Exception(MensagensEstudante.ErroAoCriarEstudante));

        // Act
        var resultado = await _useCase.ExecuteAsync(estudante.Id, dtoAtualizacao);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal(MensagensEstudante.ErroBanco, resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Dto_For_Nulo()
    {
        // Arrange
        var estudante = Data_Factory.EstudanteFaker.Generate();
        // Act
        var resultado = await _useCase.ExecuteAsync(estudante.Id, null!);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal(MensagensEstudante.ErroAoCriarEstudante
            , resultado.Mensagem);
    }
}