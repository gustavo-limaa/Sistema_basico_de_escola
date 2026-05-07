using FluentAssertions;
using Moq;
using SitemaDeMatricula.Aplicacao.Dtos.turma;
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

public class AtualizarTurmaUsecaseTestUnitario
{
    public AtualizarTurmaUsecaseTestUnitario()
    {
        // Instancia os Mocks
        _mockTurma = new Mock<IRepositorioTurma>();
        _mockdisc = new Mock<IDisciplinaRepositorio>();
        _mockprof = new Mock<IRepositorioProfessor>();

        _usecase = new AtualizarTurmaUseCase(_mockTurma.Object, _mockprof.Object, _mockdisc.Object);
    }

    private readonly Mock<IRepositorioTurma> _mockTurma;
    private readonly Mock<IRepositorioProfessor> _mockprof;
    private readonly Mock<IDisciplinaRepositorio> _mockdisc;

    private readonly AtualizarTurmaUseCase _usecase;

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
    public async Task Atualizar_Deve_Retornar_Sucesso_Quando_Dados_Sao_Validos()
    {
        // Arrange
        var idTurmaExistente = Guid.NewGuid(); // O ID da turma que vamos fingir que existe
        var dtoOriginal = CriarTUrma();
        var dtoNovo = CriarTUrma();

        // 1. Simular que a Turma EXISTE no banco antes da atualização
        var turmaExistenteNoBanco = DataFactory.TurmaFaker().Generate();
        // Forçamos o ID dela a ser o que vamos usar
        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(idTurmaExistente))
                  .ReturnsAsync(turmaExistenteNoBanco);

        // 2. Simular que o Professor e Disciplina da atualização são válidos
        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(dtoNovo.ProfessorId)).ReturnsAsync(professorFake);

        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();
        _mockdisc.Setup(d => d.ObterPorIdAsync(dtoNovo.DisciplinaId)).ReturnsAsync(disciplinaFake);

        // 3. Simular que não há conflito de código (outra turma usando o mesmo código novo)
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>())).ReturnsAsync((Turma)null);

        var turmaAtualizar = new TurmaDtoUpdate(
            ProfessorId: dtoNovo.ProfessorId,
            DisciplinaId: dtoNovo.DisciplinaId,
            Ativo: true,
            Sigla: "NEW", // Mudando algo para testar
            Semestre: 2,
            AnoLetivo: 2026,
            Numero: 100
        );

        // Act
        // Passamos o ID que o Mock "conhece"
        var resultado = await _usecase.ExecutarAsync(idTurmaExistente, turmaAtualizar);

        // Assert
        resultado.Sucesso.Should().BeTrue(because: resultado.Mensagem);
        // Dica: Verifique se o repositório chamou o Atualizar (ou SaveChanges)
        _mockTurma.Verify(t => t.AtualizarAsync(It.IsAny<Turma>()), Times.Once);
    }

    [Fact]
    public async Task Falha_por_Conflito_de_Código()
    {
        var idTurmaExistente = Guid.NewGuid();
        var turmaA = DataFactory.TurmaFaker().Generate();
        var turmaB = DataFactory.TurmaFaker().Generate();

        var DTOATULIZAR = new TurmaDtoUpdate
        (
            ProfessorId: turmaA.ProfessorId,
            DisciplinaId: turmaA.DisciplinaId,
            Ativo: true,
            Sigla: "MAT", // Mudando algo para testar
            Semestre: 2,
            AnoLetivo: 2026,
            Numero: 100

        );

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(idTurmaExistente))
                   .ReturnsAsync(turmaA);

        // 2. O PULO DO GATO: Quando o Use Case perguntar se esse código novo existe...
        // O Mock responde: "Sim, existe e pertence à Turma B!"
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                   .ReturnsAsync(turmaB);

        var resultado = await _usecase.ExecutarAsync(idTurmaExistente, DTOATULIZAR);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Este código já está sendo usado por outra turma.");
        resultado.Tipo.Should().Be(TipoErro.Conflito);
    }

    [Fact]
    public async Task Falha_por_NaoEncotrado_de_ID()
    {
        var turmaA = DataFactory.TurmaFaker().Generate();

        var DTOATULIZAR = new TurmaDtoUpdate
        (
            ProfessorId: turmaA.ProfessorId,
            DisciplinaId: turmaA.DisciplinaId,
            Ativo: true,
            Sigla: "MAT", // Mudando algo para testar
            Semestre: 2,
            AnoLetivo: 2026,
            Numero: 100

        );

        var resultado = await _usecase.ExecutarAsync(Guid.NewGuid(), DTOATULIZAR);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Turma não encontrada para atualização.");
    }

    [Fact]
    public async Task Falha_por_NaoEncotrado_de_Disciplina_ID()
    {
        // Arrange
        var turmaA = DataFactory.TurmaFaker().Generate();

        // 1. VOCÊ PRECISA DISSO: Simular que a turma que será editada existe!
        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.TurmaId))
                  .ReturnsAsync(turmaA);

        // 2. Simular que não há conflito de código (para passar pelo passo 3 do Use Case)
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null);

        // O resto do seu código de Professor e Disciplina...
        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(turmaA.ProfessorId)).ReturnsAsync(professorFake);

        _mockdisc.Setup(d => d.ObterPorIdAsync(turmaA.DisciplinaId))
                 .ReturnsAsync((Disciplina)null); // Aqui é onde queremos que falhe

        var DTOATULIZAR = new TurmaDtoUpdate(
            ProfessorId: turmaA.ProfessorId,
            DisciplinaId: turmaA.DisciplinaId,
            Ativo: true,
            Sigla: "MAT",
            Semestre: 2,
            AnoLetivo: 2026,
            Numero: 100
        );

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaA.TurmaId, DTOATULIZAR);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Disciplina não encontrada ou inativa.");
    }

    [Fact]
    public async Task Falha_por_NaoEncotrado_de_Professor_ID()
    {
        // arrange
        var turmaA = DataFactory.TurmaFaker().Generate();

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.TurmaId))
                 .ReturnsAsync(turmaA);

        // 2. Simular que não há conflito de código (para passar pelo passo 3 do Use Case)
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null);

        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar(); // Garante que está ativo
        _mockprof.Setup(p => p.ObterPorIdAsync(turmaA.ProfessorId))
                 .ReturnsAsync((Professor)null);

        var DTOATULIZAR = new TurmaDtoUpdate
        (
            ProfessorId: turmaA.ProfessorId,
            DisciplinaId: turmaA.DisciplinaId,
            Ativo: true,
            Sigla: "MAT", // Mudando algo para testar
            Semestre: 2,
            AnoLetivo: 2026,
            Numero: 100

        );
        //act
        var resultado = await _usecase.ExecutarAsync(turmaA.TurmaId, DTOATULIZAR);
        //assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Professor não encontrado ou inativo.");
    }

    [Fact]
    public async Task Falha_por_conflito_de_Discplina_Inativa()
    {
        //arrange
        var turmaA = DataFactory.TurmaFaker().Generate();
        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.TurmaId))
                 .ReturnsAsync(turmaA);

        // 2. Simular que não há conflito de código (para passar pelo passo 3 do Use Case)
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null);
        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(turmaA.ProfessorId)).ReturnsAsync(professorFake);

        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        disciplinaFake.Desativar();
        _mockdisc.Setup(d => d.ObterPorIdAsync(turmaA.DisciplinaId)).ReturnsAsync(disciplinaFake);

        var DTOATULIZAR = new TurmaDtoUpdate
        (
            ProfessorId: turmaA.ProfessorId,
            DisciplinaId: turmaA.DisciplinaId,
            Ativo: true,
            Sigla: "MAT", // Mudando algo para testar
            Semestre: 2,
            AnoLetivo: 2026,
            Numero: 100

        );
        //act
        var resultado = await _usecase.ExecutarAsync(turmaA.TurmaId, DTOATULIZAR);
        //asssert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Disciplina não encontrada por está inativa.");
    }

    [Fact]
    public async Task Falha_por_Conflito_de_Professor_Inativo()
    {
        //arrange

        var turmaA = DataFactory.TurmaFaker().Generate();
        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.TurmaId))
                 .ReturnsAsync(turmaA);

        // 2. Simular que não há conflito de código (para passar pelo passo 3 do Use Case)
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null);

        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Desativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(turmaA.ProfessorId)).ReturnsAsync(professorFake);

        var DTOATULIZAR = new TurmaDtoUpdate
        (
            ProfessorId: turmaA.ProfessorId,
            DisciplinaId: turmaA.DisciplinaId,
            Ativo: true,
            Sigla: "MAT", // Mudando algo para testar
            Semestre: 2,
            AnoLetivo: 2026,
            Numero: 100

        );
        //act
        var resultado = await _usecase.ExecutarAsync(turmaA.TurmaId, DTOATULIZAR);
        //asssert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Professor nao encontrado por está inativo.");
    }
}