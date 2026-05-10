using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.EstudanteTestUnitario;

public class AtualizarEstudanteUseCaseTest
{
    private readonly Mock<IRepositorioEstudante> _repositorioMock;
    private readonly UsesCasesAtualizarEstudante _useCaseAtualizar;

    public AtualizarEstudanteUseCaseTest()
    {
        _repositorioMock = new Mock<IRepositorioEstudante>();

        _useCaseAtualizar = new UsesCasesAtualizarEstudante(_repositorioMock.Object);
    }

    [Fact]
    public async Task Deve_Atualizar_Estudante_Quando_Dados_Sao_Validos()
    {
        // Arrange
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var idBusca = estudanteFake.Id;
        var dtoAtualizacao = DataFactory.EstudanteDtoUpdateFaker.Generate();

        // 1. Simula que achou o estudante
        _repositorioMock.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ReturnsAsync(estudanteFake);

        // 2. CONFIGURAÇÃO QUE FALTOU: Simula que o salvamento deu certo!
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(true);

        // Act
        var resultado = await _useCaseAtualizar.ExecuteAsync(idBusca, dtoAtualizacao);

        // Assert
        Assert.True(resultado.Sucesso);

        // DICA EXTRA: Verifique se o método Atualizar foi mesmo chamado
        _repositorioMock.Verify(r => r.Atualizar(It.IsAny<Estudante>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Estudante_Nao_Encontrado()
    {
        // Arrange
        var idBusca = Guid.NewGuid();
        var dtoAtualizacao = DataFactory.EstudanteDtoUpdateFaker.Generate();
        // Simula que NÃO ACHOU o estudante
        _repositorioMock.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ReturnsAsync((Estudante)null);
        // Act
        var resultado = await _useCaseAtualizar.ExecuteAsync(idBusca, dtoAtualizacao);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("Estudante não encontrado.", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Falha_Ao_Salvar()
    {
        // Arrange
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var idBusca = estudanteFake.Id;
        var dtoAtualizacao = DataFactory.EstudanteDtoUpdateFaker.Generate();
        _repositorioMock.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ReturnsAsync(estudanteFake);
        // Simula que o salvamento FALHOU
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(false);
        // Act
        var resultado = await _useCaseAtualizar.ExecuteAsync(idBusca, dtoAtualizacao);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("Falha ao atualizar o estudante.", resultado.Mensagem);

        _repositorioMock.Verify(r => r.Atualizar(It.IsAny<Estudante>()), Times.Once); // Verifica que tentou atualizar
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Ocorre_Excecao()
    {
        // Arrange
        var idBusca = Guid.NewGuid();
        var dtoAtualizacao = DataFactory.EstudanteDtoUpdateFaker.Generate();
        _repositorioMock.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ThrowsAsync(new Exception("Erro de banco de dados"));
        // Act
        var resultado = await _useCaseAtualizar.ExecuteAsync(idBusca, dtoAtualizacao);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Contains("Erro ao atualizar: Erro de banco de dados", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Id_For_Invalido()
    {
        // Arrange
        var idBusca = Guid.NewGuid(); // ID que não existe
        var dtoAtualizacao = DataFactory.EstudanteDtoUpdateFaker.Generate();
        _repositorioMock.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ReturnsAsync((Estudante)null); // Simula que não encontrou o estudante
                                                        // Act
        var resultado = await _useCaseAtualizar.ExecuteAsync(idBusca, dtoAtualizacao);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("Estudante não encontrado.", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Dto_For_Nulo()
    {
        // Arrange
        var idBusca = Guid.NewGuid(); // ID válido, mas DTO nulo
                                      // Act
        var resultado = await _useCaseAtualizar.ExecuteAsync(idBusca, null!);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("Dados de atualização inválidos.", resultado.Mensagem);
    }
}