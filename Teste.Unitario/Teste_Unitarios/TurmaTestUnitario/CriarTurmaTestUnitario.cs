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

public class CriarTurmaTestUnitario
{
    private readonly Mock<IRepositorioTurma> _mockTurma;
    private readonly Mock<IRepositorioProfessor> _mockprof;
    private readonly Mock<IDisciplinaRepositorio> _mockdisc;

    private readonly CriarTurmaUseCase _usecase;

    public CriarTurmaTestUnitario()
    {
        _mockTurma = new Mock<IRepositorioTurma>();
        _mockdisc = new Mock<IDisciplinaRepositorio>();
        _mockprof = new Mock<IRepositorioProfessor>();

        _usecase = new CriarTurmaUseCase(_mockTurma.Object, _mockprof.Object, _mockdisc.Object);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Ok_Quando_Dados_Sao_Validos()
    {
        // Arrange
        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        var disciplinaFake = Data_Factory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();

        // 🎯 DTO usa os IDs das instâncias válidas
        var dto = Data_Factory.TurmaFakerdto(professorFake.Id, disciplinaFake.Id, 12).Generate();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);
        _mockTurma.Setup(t => t.ObterPorCodigoAsync(It.IsAny<string>())).ReturnsAsync((Turma)null!);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeTrue(because: resultado.Mensagem);
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Once);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Falha_Quando_Professor_Nao_Existe()
    {
        // Arrange
        var dto = Data_Factory.TurmaFakerdto(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId))
                 .ReturnsAsync((Professor)null!);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensProfessor.ProfessorNaoEncontrado);
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Falha_Quando_Disciplina_Nao_Existe()
    {
        // Arrange
        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        var dto = Data_Factory.TurmaFakerdto(professorFake.Id, Guid.NewGuid(), 12).Generate();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync((Disciplina)null!);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensDisciplina.DisciplinaNaoEncontrada);
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Conflito_Quando_Professor_inativo()
    {
        // Arrange
        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Desativar();

        var dto = Data_Factory.TurmaFakerdto(professorFake.Id, Guid.NewGuid(), 12).Generate();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensProfessor.ErroInativo_ou_Ativo);
        resultado.Tipo.Should().Be(TipoErro.Validacao
            );
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Conflito_Quando_Disciplina_inativa()
    {
        // Arrange
        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        var disciplinaFake = Data_Factory.DisciplinaFaker.Generate();
        disciplinaFake.Desativar();

        var dto = Data_Factory.TurmaFakerdto(professorFake.Id, disciplinaFake.Id, 12).Generate();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensDisciplina.DisciplinaInativa);
        resultado.Tipo.Should().Be(TipoErro.Validacao
            );
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Conflito_Quando_Turma_Tiver_Mesmo_Codigo()
    {
        // Arrange
        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        var dto = Data_Factory.TurmaFakerdto(professorFake.Id, Guid.NewGuid(), 12).Generate();

        var turmaExistente = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        // 🎯 O Use Case consulta se o código da turma já existe antes da disciplina
        _mockTurma.Setup(t => t.ObterPorCodigoAsync(It.IsAny<string>()))
                  .ReturnsAsync(turmaExistente);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoErro.Conflito);
        resultado.Mensagem.Should().Be(MensagensTurma.TurmaJaExistente);
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Lancar_Excecao_Quando_Banco_De_Dados_Falhar()
    {
        // Arrange
        var professorFake = Data_Factory.ProfessorFaker.Generate();
        professorFake.Ativar();

        var disciplinaFake = Data_Factory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();

        var dto = Data_Factory.TurmaFakerdto(professorFake.Id, disciplinaFake.Id, 12).Generate();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);
        _mockTurma.Setup(t => t.ObterPorCodigoAsync(It.IsAny<string>())).ReturnsAsync((Turma)null!);

        _mockTurma.Setup(t => t.AdicionarAsync(It.IsAny<Turma>()))
                  .ThrowsAsync(new Exception(MensagensTurma.ErroPersistenciaBanco));

        // Act
        Func<Task> acao = async () => await _usecase.ExecutarAsync(dto);

        // Assert
        await acao.Should().ThrowAsync<Exception>()
                  .WithMessage(MensagensTurma.ErroPersistenciaBanco);
    }
}