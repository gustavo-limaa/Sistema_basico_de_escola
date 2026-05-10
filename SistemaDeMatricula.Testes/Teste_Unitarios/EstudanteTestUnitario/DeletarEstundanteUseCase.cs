using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.EstudanteTestUnitario;

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
        var estudante = DataFactory.EstudanteFaker.Generate();
        _estudanteRepository.Setup(repo => repo.ObterPorIdAsync(estudante.Id)).ReturnsAsync(estudante);
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
        Assert.Equal("Estudante não encontrado.", resultado.Mensagem); // ✅ Mensagem correta

        _estudanteRepository.Verify(repo => repo.Remover(It.IsAny<Estudante>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Houver_Erro_Ao_Deletar()
    {
        // Arrange
        var estudante = DataFactory.EstudanteFaker.Generate();
        _estudanteRepository.Setup(repo => repo.ObterPorIdAsync(estudante.Id)).ReturnsAsync(estudante);
        _estudanteRepository.Setup(repo => repo.Remover(estudante)).Throws(new Exception("Erro de banco"));
        // Act
        var resultado = await _useCaseDeletar.ExecuteAsync(estudante.Id);
        // Assert
        Assert.False(resultado.Sucesso); // ✅ O Result deve ser Falha
        Assert.Contains("Erro ao deletar estudante", resultado.Mensagem); // ✅ Mensagem de erro
        _estudanteRepository.Verify(repo => repo.ObterPorIdAsync(estudante.Id), Times.Once);
        _estudanteRepository.Verify(repo => repo.Remover(estudante), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_SalvarAlteracoes_Falhar()
    {
        // Arrange
        var estudante = DataFactory.EstudanteFaker.Generate();
        _estudanteRepository.Setup(repo => repo.ObterPorIdAsync(estudante.Id)).ReturnsAsync(estudante);
        _estudanteRepository.Setup(repo => repo.Remover(estudante));
        _estudanteRepository.Setup(repo => repo.SalvarAlteracoesAsync()).ReturnsAsync(false); // Simula falha ao salvar
        // Act
        var resultado = await _useCaseDeletar.ExecuteAsync(estudante.Id);
        // Assert
        Assert.False(resultado.Sucesso); // ✅ O Result deve ser Falha
        Assert.Equal("Falha ao deletar o estudante.", resultado.Mensagem); // ✅ Mensagem correta
        _estudanteRepository.Verify(repo => repo.ObterPorIdAsync(estudante.Id), Times.Once);
        _estudanteRepository.Verify(repo => repo.Remover(estudante), Times.Once);
        _estudanteRepository.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Once);
    }
}