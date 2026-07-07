using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.TurmaTestUnitario;

public class ResturarTurmaUsecaseTestUNI
{
    private readonly RestaurarTurmaUseCase _usecase;

    private Mock<IRepositorioTurma> _mock;

    public ResturarTurmaUsecaseTestUNI()
    {
        _mock = new Mock<IRepositorioTurma>();
        _usecase = new RestaurarTurmaUseCase(_mock.Object);
    }

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
            Numero: dto.CodigoTurma.Numero,
            CapacidadeMaxima: dto.CapacidadeMaxima

        );

        return turmaDto;
    }

    [Fact]
    public async Task Deve_Restaurar_Turma_Com_Sucesso()
    {
        // Arrange
        var turmaInativa = DataFactory.TurmaFaker().Generate();
        turmaInativa.Desativar(); // Ela começa "morta"

        _mock.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaInativa.Id))
             .ReturnsAsync(turmaInativa);

        _mock.Setup(t => t.RestaurarAsync(turmaInativa.Id))
             .ReturnsAsync(true);

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaInativa.Id);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        turmaInativa.Ativo.Should().BeTrue(); // Garante que o UseCase realmente ativou ela

        _mock.Verify(t => t.RestaurarAsync(turmaInativa.Id), Times.Once);
    }

    [Fact]
    public async Task Deve_Restaurar_Turma_e_falhar_por_id_invalido()
    {
        // Act
        var resultado = await _usecase.ExecutarAsync(Guid.NewGuid());

        // Assert
        resultado.Sucesso.Should().BeFalse
            ();

        resultado.Mensagem.Should().Be("Turma não encontrada.");
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Mesmo_Se_Turma_Ja_Estiver_Ativa()
    {
        // Arrange
        var turmaJaAtiva = DataFactory.TurmaFaker().Generate();
        turmaJaAtiva.Ativar(); // Ela já nasce ativa no teste

        _mock.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaJaAtiva.Id))
             .ReturnsAsync(turmaJaAtiva);

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaJaAtiva.Id);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        // Importante: O repositório NÃO deve ser chamado para atualizar,
        // pois não houve mudança de estado!
        _mock.Verify(t => t.RestaurarAsync(turmaJaAtiva.Id), Times.Never);
    }
}