using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.MatriculasTestUnitario;

public class ObterAllAndObterByIdMatriculaTestUnitario
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly ListarTodasMatriculasUsecase _CasoAll;
    private readonly ObterMatriculaPorIdUsecase _CasoById;

    public ObterAllAndObterByIdMatriculaTestUnitario()
    {
        _uowMock = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };
        _CasoAll = new ListarTodasMatriculasUsecase(_uowMock.Object);
        _CasoById = new ObterMatriculaPorIdUsecase(_uowMock.Object);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Obter_Todas_Matriculas()
    {
        // Arrange - Gera 2 matrículas puras em memória instantaneamente
        var matriculasFake = DataFactory.MatriculaFaker.Generate(2);

        _uowMock.Setup(r => r.Matriculas.ListarTodasAsync())
                .ReturnsAsync(matriculasFake);

        // Act
        var resultado = await _CasoAll.ExecutarAsync();

        // Assert
        resultado.Sucesso.Should().BeTrue(); // Assumindo que você usa o Result Pattern
        resultado.Dados.Should().HaveCount(2);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Obter_Matricula_Por_Id()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        var matriculaFake = DataFactory.MatriculaFaker.Generate();

        _uowMock.Setup(r => r.Matriculas.ObterPorIdAsync(matriculaId))
                .ReturnsAsync(matriculaFake);

        // Act
        var resultado = await _CasoById.ExecutarAsync(matriculaId);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        resultado.Dados.Should().NotBeNull();
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Obter_Matricula_Por_Id_Inexistente()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        _uowMock.Setup(r => r.Matriculas.ObterPorIdAsync(matriculaId))
                .ReturnsAsync((Matricula)null); // Simula matrícula não encontrada
                                                // Act
        var resultado = await _CasoById.ExecutarAsync(matriculaId);
        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("Matrícula não encontrada.");
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Com_Lista_Vazia_Quando_Nao_Houver_Matriculas()
    {
        // Arrange
        _uowMock.Setup(r => r.Matriculas.ListarTodasAsync())
                .ReturnsAsync(new List<Matricula>()); // Lista vazia vinda do banco

        // Act
        var resultado = await _CasoAll.ExecutarAsync();

        // Assert
        resultado.Sucesso.Should().BeTrue();
        resultado.Dados.Should().BeEmpty(); // Garante que a lista veio vazia (0 elementos), sem quebrar!
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Obter_Matricula_Por_Id_Com_Id_Invalido()
    {
        // Arrange
        var matriculaId = Guid.Empty;
        _uowMock.Setup(r => r.Matriculas.ObterPorIdAsync(matriculaId))
                .ReturnsAsync((Matricula)null); // Simula matrícula não encontrada
                                                // Act
        var resultado = await _CasoById.ExecutarAsync(matriculaId);
        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("O identificador da matrícula é obrigatório.");
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Obter_Todas_Matriculas_Com_Erro_No_Banco()
    {
        // Arrange
        _uowMock.Setup(r => r.Matriculas.ListarTodasAsync())
                .ThrowsAsync(new Exception("Erro de conexão com o banco de dados"));
        // Act
        var resultado = await _CasoAll.ExecutarAsync();
        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("Ocorreu um erro ao listar as matrículas: Erro de conexão com o banco de dados");
    }
}