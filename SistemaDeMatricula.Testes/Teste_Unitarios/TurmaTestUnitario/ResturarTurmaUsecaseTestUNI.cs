using Bogus.Extensions.UnitedKingdom;
using FluentAssertions;
using Moq;
using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Aplicacao.Usecases.Professor;
using SitemaDeMatricula.Aplicacao.Usecases.Turmas;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Numero: dto.CodigoTurma.Numero
        );

        return turmaDto;
    }

    [Fact]
    public async Task Deve_Restaurar_Turma_Com_Sucesso()
    {
        // Arrange
        var turmaInativa = DataFactory.TurmaFaker().Generate();
        turmaInativa.Desativar(); // Ela começa "morta"

        _mock.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaInativa.TurmaId))
             .ReturnsAsync(turmaInativa);

        _mock.Setup(t => t.AtualizarAsync(turmaInativa))
             .ReturnsAsync(true);

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaInativa.TurmaId);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        turmaInativa.Ativo.Should().BeTrue(); // Garante que o UseCase realmente ativou ela

        _mock.Verify(t => t.AtualizarAsync(turmaInativa), Times.Once);
    }

    [Fact]
    public async Task Deve_Restaurar_Turma_e_falhar_por_id_invalido()
    {
        // Act
        var resultado = await _usecase.ExecutarAsync(Guid.NewGuid());

        // Assert
        resultado.Sucesso.Should().BeFalse
            ();

        resultado.Mensagem.Should().Be("Turma não encontrada ou já está ativa.");
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Mesmo_Se_Turma_Ja_Estiver_Ativa()
    {
        // Arrange
        var turmaJaAtiva = DataFactory.TurmaFaker().Generate();
        turmaJaAtiva.Ativar(); // Ela já nasce ativa no teste

        _mock.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaJaAtiva.TurmaId))
             .ReturnsAsync(turmaJaAtiva);

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaJaAtiva.TurmaId);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        // Importante: O repositório NÃO deve ser chamado para atualizar,
        // pois não houve mudança de estado!
        _mock.Verify(t => t.AtualizarAsync(It.IsAny<Turma>()), Times.Never);
    }
}