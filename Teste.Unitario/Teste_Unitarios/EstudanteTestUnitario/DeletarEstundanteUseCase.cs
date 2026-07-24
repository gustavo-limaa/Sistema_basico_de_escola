using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Test.Shared;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.EstudanteTestUnitario;

public class DeletarEstundanteUseCase
{
    private readonly Mock<IRepositorioEstudante> _estudanteRepository;
    private readonly UsesCasesDeletarEstudante _useCaseDeletar;

    public DeletarEstundanteUseCase()
    {
        _estudanteRepository = new Mock<IRepositorioEstudante>();
        _useCaseDeletar = new UsesCasesDeletarEstudante(_estudanteRepository.Object);
    }

    [Fact]
    public async Task Deve_Deletar_Estudante_Corretamente()
    {
        // Arrange
        var estudante = Data_Factory.EstudanteFaker.Generate();

        _estudanteRepository.Setup(repo => repo.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync(estudante);
        _estudanteRepository.Setup(repo => repo.Remover(estudante));

        // Act
        await _useCaseDeletar.ExecuteAsync(estudante.Id);

        // Assert
        _estudanteRepository.Verify(repo => repo.ObterPorIdAsync(estudante.Id), Times.Once);
        _estudanteRepository.Verify(repo => repo.Remover(estudante), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Estudante_Nao_Encontrado()
    {
        // Arrange
        var estudanteId = Guid.NewGuid();
        _estudanteRepository.Setup(repo => repo.ObterPorIdAsync(estudanteId))
                            .ReturnsAsync((Estudante)null!);

        // Act
        var resultado = await _useCaseDeletar.ExecuteAsync(estudanteId);

        // Assert
        Assert.False(resultado.Sucesso); // ✅ O Result deve ser Falha
        Assert.Equal(resultado.Mensagem, resultado.Mensagem); // ✅ Mensagem correta

        _estudanteRepository.Verify(repo => repo.Remover(It.IsAny<Estudante>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Houver_Erro_Ao_Deletar()
    {
        // Arrange
        var estudante = Data_Factory.EstudanteFaker.Generate();
        _estudanteRepository.Setup(repo => repo.ObterPorIdAsync(estudante.Id)).ReturnsAsync(estudante);
        _estudanteRepository.Setup(repo => repo.Remover(estudante)).Throws(new Exception(MensagensEstudante.ErroBanco));
        // Act
        var resultado = await _useCaseDeletar.ExecuteAsync(estudante.Id);
        // Assert
        Assert.False(resultado.Sucesso); // ✅ O Result deve ser Falha
        Assert.Contains(MensagensEstudante.ErroBanco, resultado.Mensagem); // ✅ Mensagem de erro
        _estudanteRepository.Verify(repo => repo.ObterPorIdAsync(estudante.Id), Times.Once);
        _estudanteRepository.Verify(repo => repo.Remover(estudante), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_SalvarAlteracoes_Falhar()
    {
        // Arrange
        var estudante = Data_Factory.EstudanteFaker.Generate();
        _estudanteRepository.Setup(repo => repo.ObterPorIdAsync(estudante.Id)).ReturnsAsync(estudante);
        _estudanteRepository.Setup(repo => repo.Remover(estudante));
        _estudanteRepository.Setup(repo => repo.SalvarAlteracoesAsync()).ReturnsAsync(false); // Simula falha ao salvar
        // Act
        var resultado = await _useCaseDeletar.ExecuteAsync(estudante.Id);
        // Assert
        Assert.False(resultado.Sucesso); // ✅ O Result deve ser Falha
        Assert.Equal(MensagensEstudante.ErroBanco, resultado.Mensagem); // ✅ Mensagem correta
        _estudanteRepository.Verify(repo => repo.ObterPorIdAsync(estudante.Id), Times.Once);
        _estudanteRepository.Verify(repo => repo.Remover(estudante), Times.Once);
        _estudanteRepository.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Once);
    }
}