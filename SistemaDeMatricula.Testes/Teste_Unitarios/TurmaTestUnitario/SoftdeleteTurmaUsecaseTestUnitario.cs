using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.TurmaTestUnitario
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
        public async Task Deve_Retornar_Falha_Quando_Turma_Tiver_Alunos_Ativos()
        {
            // Arrange
            var turmaA = DataFactory.TurmaFaker().Generate();
            turmaA.Ativar();

            _mockTurma.Setup(t => t.ObterPorIdAsync(turmaA.Id)).ReturnsAsync(turmaA);

            // O PULO DO GATO: Simula que o repositório achou alunos
            _mockMatri.Setup(m => m.ExisteQualquerMatriculaAtivaParaTurmaAsync(turmaA.Id))
                          .ReturnsAsync(true);

            // Act
            var resultado = await _usecase.ExecutarAsync(turmaA.Id);

            // Assert
            resultado.Sucesso.Should().BeFalse();
            resultado.Mensagem.Should().Be("Não é possível desativar uma turma com alunos matriculados.");

            // Garante que o Atualizar NUNCA foi chamado (segurança total)
            _mockTurma.Verify(t => t.AtualizarAsync(It.IsAny<Turma>()), Times.Never);
        }

        [Fact]
        public async Task Deve_Desativar_Turma_Com_Sucesso_Quando_Nao_Houver_Alunos()
        {
            // Arrange
            var turmaA = DataFactory.TurmaFaker().Generate();
            turmaA.Ativar();

            _mockTurma.Setup(t => t.ObterPorIdAsync(turmaA.Id)).ReturnsAsync(turmaA);
            _mockMatri.Setup(m => m.ExisteQualquerMatriculaAtivaParaTurmaAsync(turmaA.Id))
                          .ReturnsAsync(false); // Liberado!

            // Act
            var resultado = await _usecase.ExecutarAsync(turmaA.Id);

            // Assert
            resultado.Sucesso.Should().BeTrue();
            turmaA.Ativo.Should().BeFalse(); // Verifica se o Soft Delete aconteceu no objeto
            _mockTurma.Verify(t => t.AtualizarAsync(turmaA), Times.Once);
        }
    }
}