using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Test.Shared;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.TurmaTestUnitario
{
    public class SoftdeleteTurmaUsecaseTestUnitario
    {
        public SoftdeleteTurmaUsecaseTestUnitario()
        {
            // Instancia os Mocks
            _mockTurma = new Mock<IRepositorioTurma>();
            _mockestu = new Mock<IRepositorioEstudante>();
            _mockMatri = new Mock<IRepositorioMatricula>();

            _usecase = new RemoverTurmaUseCase(_mockTurma.Object, _mockMatri.Object
                , _mockestu.Object);
        }

        private readonly Mock<IRepositorioTurma> _mockTurma;
        private readonly Mock<IRepositorioEstudante> _mockestu;
        private readonly Mock<IRepositorioMatricula> _mockMatri;

        private readonly RemoverTurmaUseCase _usecase;

        [Fact]
        public async Task Deve_Retornar_Falha_Quando_Turma_Tiver_Alunos_Ativos()
        {
            // Arrange
            var turmaA = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();
            turmaA.Ativar();

            // 🎯 O PULO DO GATO: Aceita qualquer Guid para não falhar por divergência de referência
            _mockTurma.Setup(t => t.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync(turmaA);
            // Caso seu use case use o de filtros, mocke ele também:
            _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(It.IsAny<Guid>())).ReturnsAsync(turmaA);

            _mockMatri.Setup(m => m.ExisteQualquerMatriculaAtivaParaTurmaAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);

            // Act
            var resultado = await _usecase.ExecutarAsync(turmaA.Id);

            // Assert
            resultado.Sucesso.Should().BeFalse(because: resultado.Mensagem);
            resultado.Mensagem.Should().Be(MensagensTurma.TurmaComAlunosMatriculados);
            _mockTurma.Verify(t => t.AtualizarAsync(It.IsAny<Turma>()), Times.Never);
        }

        [Fact]
        public async Task Deve_Desativar_Turma_Com_Sucesso_Quando_Nao_Houver_Alunos()
        {
            // Arrange
            var turmaA = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();
            turmaA.Ativar();

            _mockTurma.Setup(t => t.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync(turmaA);
            _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(It.IsAny<Guid>())).ReturnsAsync(turmaA);
            _mockTurma.Setup(t => t.SalvarAlteracoesAsync()).ReturnsAsync(true); // Se o use case chama SaveChanges!

            _mockMatri.Setup(m => m.ExisteQualquerMatriculaAtivaParaTurmaAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(false);

            // Act
            var resultado = await _usecase.ExecutarAsync(turmaA.Id);

            // Assert
            resultado.Sucesso.Should().BeTrue(because: resultado.Mensagem);
            turmaA.Ativo.Should().BeFalse();
        }
    }
}