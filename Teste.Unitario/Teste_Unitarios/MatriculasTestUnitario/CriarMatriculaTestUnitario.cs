using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Value_Object;
using SistemaDeMatricula.Services;
using SistemaDeMatricula.Test.Shared;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.MatriculasTestUnitario;

public class CriarMatriculaTestUnitario
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRabbitMqProducer> _rabbitMock;
    private readonly MatricularEstudanteUsecase _useCase;

    public CriarMatriculaTestUnitario()
    {
        _uowMock = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };

        _rabbitMock = new Mock<IRabbitMqProducer> { DefaultValue = DefaultValue.Mock };

        _rabbitMock.Setup(x => x.EnviarMensagemAsync(It.IsAny<It.IsAnyType>(), It.IsAny<string>()))
                   .Returns(Task.CompletedTask);

        _useCase = new MatricularEstudanteUsecase(_uowMock.Object, _rabbitMock.Object);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Criar_Matricula_Com_Sucesso()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);

        var estudanteFake = Data_Factory.EstudanteFaker.Generate();
        var codigoTurma = new CodigoTurma("CSH", 2026, 1, 1);
        var turmaFake = new Turma(codigoTurma, Guid.NewGuid(), Guid.NewGuid(), capacidadeMaxima: 30);

        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync(estudanteFake);

        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaId))
                .ReturnsAsync(turmaFake);

        _uowMock.Setup(r => r.Matriculas.ExisteMatriculaAtivaAsync(estudanteId, turmaId))
                .ReturnsAsync(false);

        _uowMock.Setup(r => r.Matriculas.ContarMatriculasAtivasNaTurmaAsync(turmaId))
                .ReturnsAsync(10);

        _uowMock.Setup(r => r.CommitAsync())
                .ReturnsAsync(true);

        var resultado = await _useCase.ExecutarAsync(dto);

        resultado.Sucesso.Should().BeTrue();
        resultado.Dados.Should().NotBeNull();
        resultado.Dados.EstudanteId.Should().Be(estudanteId);
        resultado.Dados.TurmaId.Should().Be(turmaId);

        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Once);
        _uowMock.Verify(r => r.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Turma_Estiver_Lotada()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);
        var estudanteFake = Data_Factory.EstudanteFaker.Generate();
        var codigoTurma = new CodigoTurma("CSH", 2026, 1, 1);
        var turmaFake = new Turma(codigoTurma, Guid.NewGuid(), Guid.NewGuid(), capacidadeMaxima: 30);

        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync(estudanteFake);
        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaId))
                .ReturnsAsync(turmaFake);
        _uowMock.Setup(r => r.Matriculas.ExisteMatriculaAtivaAsync(estudanteId, turmaId))
                .ReturnsAsync(false);
        _uowMock.Setup(r => r.Matriculas.ContarMatriculasAtivasNaTurmaAsync(turmaId))
                .ReturnsAsync(30);

        var resultado = await _useCase.ExecutarAsync(dto);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensTurma.TurmaLotada);
        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Estudante_Nao_Existir()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);

        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync((Estudante)null);
        var resultado = await _useCase.ExecutarAsync(dto);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensEstudante.ErroEstudanteNaoEncontrado);

        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Turma_Nao_Existir()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);
        var estudanteFake = Data_Factory.EstudanteFaker.Generate();
        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync(estudanteFake);
        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaId))
                .ReturnsAsync((Turma)null);
        var resultado = await _useCase.ExecutarAsync(dto);
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensTurma.TurmaNaoEncontrada);
        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Estudante_Ja_Estiver_Matriculado_Na_Turma()
    {
        var estudanteFake = Data_Factory.EstudanteFaker.Generate();
        var turmaFake = Data_Factory.TurmaFaker(Guid.NewGuid(), Guid.NewGuid(), 12).Generate();

        var dto = new MatriculaDtoCreate(estudanteFake.Id, turmaFake.Id); // 👈 usa os Ids reais dos fakes

        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteFake.Id))
                .ReturnsAsync(estudanteFake);
        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaFake.Id))
                .ReturnsAsync(turmaFake);
        _uowMock.Setup(r => r.Matriculas.ExisteMatriculaAtivaAsync(estudanteFake.Id, turmaFake.Id))
                .ReturnsAsync(true);

        var resultado = await _useCase.ExecutarAsync(dto);

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be(MensagensMatricula.MatriculaJaExistente);

        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }
}