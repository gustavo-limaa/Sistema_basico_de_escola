using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Teste.Unit.Teste_Unitarios;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.TurmaTestUnitario;

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

    [Fact]
    public async Task Atualizar_Deve_Retornar_Sucesso_Quando_Dados_Sao_Validos()
    {
        // Arrange
        var idTurmaExistente = Guid.NewGuid();

        // 1. Criamos os Mocks do Professor e da Disciplina primeiro
        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        var disciplinaFake = Data_Factory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();

        // 2. 🎯 O PULO DO GATO: Criamos o DTO usando os IDs do professorFake e da disciplinaFake!
        var turmaAtualizar = Data_Factory.TurmaFakerup(professorFake.Id, disciplinaFake.Id, 12).Generate();

        // 3. Simular que a Turma EXISTE no banco
        var turmaExistenteNoBanco = Data_Factory.TurmaFaker(professorFake.Id, disciplinaFake.Id, 12).Generate();
        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(idTurmaExistente))
                  .ReturnsAsync(turmaExistenteNoBanco);

        // 4. Mocks configurados apontando para os IDs do turmaAtualizar
        _mockprof.Setup(p => p.ObterPorIdAsync(turmaAtualizar.ProfessorId)).ReturnsAsync(professorFake);
        _mockdisc.Setup(d => d.ObterPorIdAsync(turmaAtualizar.DisciplinaId)).ReturnsAsync(disciplinaFake);

        // 5. Simular sem conflito de código e com persistência com sucesso
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>())).ReturnsAsync((Turma)null!);
        _mockTurma.Setup(t => t.SalvarAlteracoesAsync()).ReturnsAsync(true);

        // Act
        var resultado = await _usecase.ExecutarAsync(idTurmaExistente, turmaAtualizar);

        // Assert
        resultado.Sucesso.Should().BeTrue(because: resultado.Mensagem);
        _mockTurma.Verify(t => t.AtualizarAsync(It.IsAny<Turma>()), Times.Once);
    }

    [Fact]
    public async Task Falha_por_Conflito_de_Código()
    {
        // Arrange
        var idTurmaExistente = Guid.NewGuid();
        var turmaA = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();
        var turmaB = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        var DTOATUALIZAR = Data_Factory.TurmaFakerup(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(idTurmaExistente))
                  .ReturnsAsync(turmaA);

        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync(turmaB);

        // Act
        var resultado = await _usecase.ExecutarAsync(idTurmaExistente, DTOATUALIZAR);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensTurma.TurmaJaExistente); // 🎯 Mensagem real do Use Case!
        resultado.Tipo.Should().Be(TipoErro.Conflito);
    }

    [Fact]
    public async Task Falha_por_NaoEncontrado_de_ID()
    {
        // Arrange
        var DTOATUALIZAR = Data_Factory.TurmaFakerup(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(It.IsAny<Guid>()))
                  .ReturnsAsync((Turma)null!);

        // Act
        var resultado = await _usecase.ExecutarAsync(Guid.NewGuid(), DTOATUALIZAR);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensTurma.Invalida);
    }

    [Fact]
    public async Task Falha_por_NaoEncotrado_de_Professor_ID()
    {
        // arrange
        var turmaA = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.Id))
                 .ReturnsAsync(turmaA);

        // 2. Simular que não há conflito de código (para passar pelo passo 3 do Use Case)
        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null);

        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar(); // Garante que está ativo
        _mockprof.Setup(p => p.ObterPorIdAsync(turmaA.ProfessorId))
                 .ReturnsAsync((Professor)null);

        var DTOATULIZAR = Data_Factory.TurmaFakerup(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();
        //act
        var resultado = await _usecase.ExecutarAsync(turmaA.Id, DTOATULIZAR);
        //assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain(MensagensProfessor.ProfessorNaoEncontrado);
    }

    [Fact]
    public async Task Falha_por_conflito_de_Discplina_Inativa()
    {
        // Arrange
        var turmaA = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        var disciplinaFake = Data_Factory.DisciplinaFaker.Generate();
        disciplinaFake.Desativar(); // Disciplina criada como inativa

        // 🎯 O PULO DO GATO: DTO montado com os IDs exatos dos Mocks!
        var DTOATUALIZAR = Data_Factory.TurmaFakerup(professorFake.Id, disciplinaFake.Id, 12).Generate();

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.Id))
                  .ReturnsAsync(turmaA);

        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null!);

        // Mocks escutando exatamente o que vem no DTOATUALIZAR
        _mockprof.Setup(p => p.ObterPorIdAsync(DTOATUALIZAR.ProfessorId))
                 .ReturnsAsync(professorFake);

        _mockdisc.Setup(d => d.ObterPorIdAsync(DTOATUALIZAR.DisciplinaId))
                 .ReturnsAsync(disciplinaFake);

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaA.Id, DTOATUALIZAR);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        // 🎯 O Use Case retorna "DisciplinaNaoEncontrada" para disciplina inativa!
        resultado.Mensagem.Should().Be(MensagensDisciplina.DisciplinaNaoEncontrada);
    }

    [Fact]
    public async Task Falha_por_NaoEncotrado_de_Disciplina_ID()
    {
        // Arrange
        var turmaA = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        // 🎯 O DTO envia o ID do professor válido e um Guid aleatório para a disciplina que não existe
        var idDisciplinaInexistente = Guid.NewGuid();
        var DTOATUALIZAR = Data_Factory.TurmaFakerup(professorFake.Id, idDisciplinaInexistente, 12).Generate();

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.Id))
                  .ReturnsAsync(turmaA);

        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null!);

        // Professor encontrado com sucesso para passar do 1º if
        _mockprof.Setup(p => p.ObterPorIdAsync(DTOATUALIZAR.ProfessorId))
                 .ReturnsAsync(professorFake);

        // Disciplina retorna NULL para acionar a falha esperada
        _mockdisc.Setup(d => d.ObterPorIdAsync(DTOATUALIZAR.DisciplinaId))
                 .ReturnsAsync((Disciplina)null!);

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaA.Id, DTOATUALIZAR);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensDisciplina.DisciplinaNaoEncontrada);
    }

    [Fact]
    public async Task Falha_por_Conflito_de_Professor_Inativo()
    {
        // Arrange
        var turmaA = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();
        var professorInativo = Data_Factory.ProfessorFaker.Generate();
        professorInativo.Desativar();

        var DTOATUALIZAR = Data_Factory.TurmaFakerup(professorInativo.Id, Guid.NewGuid(), 12).Generate();

        _mockTurma.Setup(t => t.ObterPorIdIgnorandoFiltrosAsync(turmaA.Id))
                  .ReturnsAsync(turmaA);

        _mockTurma.Setup(t => t.ObterPorCodigoIgnorandoFiltrosAsync(It.IsAny<string>()))
                  .ReturnsAsync((Turma)null!);

        _mockprof.Setup(p => p.ObterPorIdAsync(DTOATUALIZAR.ProfessorId))
                 .ReturnsAsync(professorInativo);

        // Act
        var resultado = await _usecase.ExecutarAsync(turmaA.Id, DTOATUALIZAR);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensProfessor.ProfessorNaoEncontrado); // 🎯 Alinhado com o Use Case!
        resultado.Tipo.Should().Be(TipoErro.Conflito);
    }
}