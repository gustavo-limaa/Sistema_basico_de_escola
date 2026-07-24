using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Teste.Unit.Teste_Unitarios;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.TurmaTestUnitario;

public class PegarAndPegarPorIdTurmaUsecases
{
    public PegarAndPegarPorIdTurmaUsecases()
    {
        // Instancia os Mocks
        _mockTurma = new Mock<IRepositorioTurma>();
        _mockdisc = new Mock<IDisciplinaRepositorio>();
        _mockprof = new Mock<IRepositorioProfessor>();

        // Initializer dos UseCases
        InicializarServicos();
    }

    private void InicializarServicos()
    {
        // Aqui você centraliza a criação
        _usecase = new ListarTurmaUsecase(_mockTurma.Object, _mockprof.Object, _mockdisc.Object);
        _usecaseID = new ObterPorIdTurma(_mockTurma.Object);
        _usecaseCode = new ObterPorCodigoTurma(_mockTurma.Object);
    }

    private readonly Mock<IRepositorioTurma> _mockTurma;
    private readonly Mock<IRepositorioProfessor> _mockprof;
    private readonly Mock<IDisciplinaRepositorio> _mockdisc;

    private ListarTurmaUsecase _usecase;
    private ObterPorIdTurma _usecaseID;
    private ObterPorCodigoTurma _usecaseCode;

    [Fact]
    public async Task Lista_todas_As_Turma_Cadastradas()
    {
        var lista = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate(10);

        _mockTurma.Setup(t => t.ListarTodasAsync())
          .Returns(Task.FromResult<IEnumerable<Turma>>(lista));

        var resultado = await _usecase.ExecutarAsync();
        // Assert
        Assert.NotNull(resultado);
        Assert.NotNull(resultado.Dados);
        Assert.Equal(10, resultado.Dados.Count());
        resultado.Sucesso.Should().BeTrue();
        _mockTurma.Verify(t => t.ListarTodasAsync(), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Houver_Erro_Interno_PegarPorId()
    {
        // Arrange
        var idBusca = Guid.NewGuid();
        _mockTurma.Setup(r => r.ObterPorIdAsync(idBusca))
                        .ThrowsAsync(new Exception(MensagensTurma.TurmaNaoEncontrada));
        // Act
        var resultado = await _usecaseID.ExecutarAsync
            (idBusca);
        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Contains("", resultado.Mensagem); _mockTurma.Verify(t => t.ObterPorIdAsync
        (It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Id_Valido_PegarPorId()
    {
        // Arrange
        var turmaFake = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();
        turmaFake.Ativar(); // Garante que a regra !turma.Ativo não barre o teste

        // AJUSTE AQUI: Use o nome exato do método que está no Use Case
        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaFake.Id))
          .Returns(Task.FromResult<Turma?>(turmaFake));

        // Act
        var resultado = await _usecaseID.ExecutarAsync(turmaFake.Id);

        // Assert
        resultado.Sucesso.Should().BeTrue(because: resultado.Mensagem);
        resultado.Dados.Id.Should().Be(turmaFake.Id); _mockTurma.Verify(t => t.ObterPorIdIgnorandoFiltrosAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Codigo_Valido()
    {
        // Arrange
        var turmaFake = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();
        var codigoBusca = turmaFake.CodigoTurma.ValorFormatado; // A string "MAT-2026-1-001"

        // IMPORTANTE: O Setup precisa casar com a string de busca
        _mockTurma.Setup(t => t.ObterPorCodigoAsync(codigoBusca))
                  .Returns(Task.FromResult<Turma?>(turmaFake));

        // Act
        var resultado = await _usecaseCode.ExecutarAsync(codigoBusca);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        resultado.Dados.CodigoFormatado.Should().Be(codigoBusca);
        _mockTurma.Verify(t => t.ObterPorCodigoAsync(It.IsAny<string>()), Times.Once);
    }
}