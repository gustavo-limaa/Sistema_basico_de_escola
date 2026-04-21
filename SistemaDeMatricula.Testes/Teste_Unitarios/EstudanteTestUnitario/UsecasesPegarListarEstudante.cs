using Moq;
using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Aplicacao.Usecases.Estudante;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Objetc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.EstudanteTestUnitario;

public class UsecasesPegarListarEstudante
{
    private readonly Mock<IRepositorioEstudante> _repositorioMock;
    private readonly UsesCasesListarTodosEstudante _useCase;
    private readonly UsesCasesPegarPorIdEstudante _useCasePegarPorId;

    // REMOVI OS PARÂMETROS DAQUI:
    public UsecasesPegarListarEstudante()
    {
        // Agora o xUnit consegue entrar aqui e rodar essas linhas
        _repositorioMock = new Mock<IRepositorioEstudante>();
        _useCase = new UsesCasesListarTodosEstudante(_repositorioMock.Object);
        _useCasePegarPorId = new UsesCasesPegarPorIdEstudante(_repositorioMock.Object);
    }

    [Fact]
    public async Task Deve_Retornar_Lista_Vazia_Quando_Nao_Houver_Estudantes()
    {
        // Arrange
        _repositorioMock.Setup(r => r.ObterTodosAsync())
                        .ReturnsAsync(new List<Estudante>());

        // Act
        var resultado = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.NotNull(resultado.Dados);
        Assert.Empty(resultado.Dados);
    }

    [Fact]
    public async Task Deve_Retornar_Estudante_Quando_Existir_Id()
    {
        // Arrange
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        // Se a sua Factory já gera um ID, você pode usar o que já vem nela:
        var idBusca = estudanteFake.EstudanteId;

        _repositorioMock.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ReturnsAsync(estudanteFake);

        // Act
        var resultado = await _useCasePegarPorId.ExecuteAsync(idBusca);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Dados);

        // Compara o ID
        Assert.Equal(idBusca, resultado.Dados.EstudanteId);

        // COMPARAÇÃO SEGURA: Use .ToString() ou a propriedade de valor do VO
        // para garantir que você está comparando texto com texto
        Assert.Equal(estudanteFake.NomeCompleto.Valor, resultado.Dados.NomeCompleto);
        Assert.Equal(estudanteFake.Email.Valor, resultado.Dados.Email);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Id_Nao_Existir_No_Banco()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Configuramos o Mock para retornar NULL
        _repositorioMock.Setup(r => r.ObterPorIdAsync(idInexistente))
                        .ReturnsAsync((Estudante)null!);

        // Act
        var resultado = await _useCasePegarPorId.ExecuteAsync(idInexistente);

        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal("Estudante não encontrado.", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retornar_Lista_De_Estudantes_Quando_Houver_Estudantes()
    {
        // Arrange
        var estudantesFake = DataFactory.EstudanteFaker.Generate(3);
        _repositorioMock.Setup(r => r.ObterTodosAsync())
                        .ReturnsAsync(estudantesFake);
        // Act
        var resultado = await _useCase.ExecuteAsync();
        // Assert
        Assert.NotNull(resultado);
        Assert.NotNull(resultado.Dados);
        Assert.Equal(3, resultado.Dados.Count());
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Houver_Erro_Interno()
    {
        // Arrange
        _repositorioMock.Setup(r => r.ObterTodosAsync())
                        .ThrowsAsync(new Exception("Erro de banco de dados"));
        // Act
        var resultado = await _useCase.ExecuteAsync();
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Contains("Erro ao listar estudantes: Erro de banco de dados", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Houver_Erro_Interno_PegarPorId()
    {
        // Arrange
        var idBusca = Guid.NewGuid();
        _repositorioMock.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ThrowsAsync(new Exception("Erro de banco de dados"));
        // Act
        var resultado = await _useCasePegarPorId.ExecuteAsync(idBusca);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Contains("Erro ao obter estudante por ID: Erro de banco de dados", resultado.Mensagem);
    }
}