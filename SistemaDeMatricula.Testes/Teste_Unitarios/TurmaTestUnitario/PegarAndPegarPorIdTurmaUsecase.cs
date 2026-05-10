using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.TurmaTestUnitario;

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

    private TurmaDtoCreate CriarTUrma()
    {
        var dto = DataFactory.TurmaFaker().Generate();

        var turmaDto = new TurmaDtoCreate
        (
            DisciplinaId: dto.DisciplinaId,
            ProfessorId: dto.ProfessorId,
            Sigla: dto.CodigoTurma.Sigla,
            Semestre: dto.CodigoTurma.Semestre,
            AnoLetivo: dto.CodigoTurma.Ano,
            Numero: dto.CodigoTurma.Numero
        );

        return turmaDto;
    }

    [Fact]
    public async Task Lista_todas_As_Turma_Cadastradas()
    {
        var lista = DataFactory.GerarListaDeTurmas(10);

        _mockTurma.Setup(t => t.ListarTodasAsync()
        ).ReturnsAsync(lista);

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
                        .ThrowsAsync(new Exception("Turma não encontrada no sistema."));
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
        var turmaFake = DataFactory.TurmaFaker().Generate();
        turmaFake.Ativar(); // Garante que a regra !turma.Ativo não barre o teste

        // AJUSTE AQUI: Use o nome exato do método que está no Use Case
        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaFake.Id))
                  .ReturnsAsync(turmaFake);

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
        var turmaFake = DataFactory.TurmaFaker().Generate();
        var codigoBusca = turmaFake.CodigoTurma.ValorFormatado; // A string "MAT-2026-1-001"

        // IMPORTANTE: O Setup precisa casar com a string de busca
        _mockTurma.Setup(t => t.ObterPorCodigoAsync(codigoBusca))
                  .ReturnsAsync(turmaFake);

        // Act
        var resultado = await _usecaseCode.ExecutarAsync(codigoBusca);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        resultado.Dados.CodigoFormatado.Should().Be(codigoBusca);
        _mockTurma.Verify(t => t.ObterPorCodigoAsync(It.IsAny<string>()), Times.Once);
    }
}