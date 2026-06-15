using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Value_Object;
using SistemaDeMatricula.Services;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.MatriculasTestUnitario;

public class CriarMatriculaTestUnitario
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRabbitMqProducer> _rabbitMock;
    private readonly MatricularEstudanteUsecase _useCase;

    public CriarMatriculaTestUnitario()
    {
        _uowMock = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };

        // 1. Criamos o mock da INTERFACE do produtor (Troque pelo nome exato da sua interface)
        _rabbitMock = new Mock<IRabbitMqProducer> { DefaultValue = DefaultValue.Mock };

        // 2. Configuramos o mock para apenas fingir que executou com sucesso (Task completada)
        _rabbitMock.Setup(x => x.EnviarMensagemAsync(It.IsAny<object>(), It.IsAny<string>()))
                   .Returns(Task.CompletedTask);

        // 3. Injetamos o .Object do mock no UseCase. Agora ele está 100% isolado da rede!
        _useCase = new MatricularEstudanteUsecase(_uowMock.Object, _rabbitMock.Object);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Criar_Matricula_Com_Sucesso()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);

        // Instanciamos objetos reais em memória para os repositórios retornarem
        // (Ajuste os parâmetros dos construtores conforme as suas entidades reais)
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var codigoTurma = new CodigoTurma("CSH", 2026, 1, 1);
        var turmaFake = new Turma(codigoTurma, Guid.NewGuid(), Guid.NewGuid(), capacidadeMaxima: 30);

        // 🎭 Ensinando os dublês (Mocks) como responder ao Use Case:

        // 1. O estudante existe no banco
        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync(estudanteFake);

        // 2. A turma existe no banco
        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaId))
                .ReturnsAsync(turmaFake);

        // 3. Ele NÃO está matriculado ainda nessa turma
        _uowMock.Setup(r => r.Matriculas.ExisteMatriculaAtivaAsync(estudanteId, turmaId))
                .ReturnsAsync(false);

        // 4. A turma só tem 10 alunos matriculados (ou seja, tem vaga livre!)
        _uowMock.Setup(r => r.Matriculas.ContarMatriculasAtivasNaTurmaAsync(turmaId))
                .ReturnsAsync(10);

        // 5. O commit do Unit of Work vai salvar com sucesso
        _uowMock.Setup(r => r.CommitAsync())
                .ReturnsAsync(true);

        // ==========================================
        // ACT: Disparando a ação do Use Case
        // ==========================================
        var resultado = await _useCase.ExecutarAsync(dto);

        // ==========================================
        // ASSERT: Conferindo o resultado e comportamento
        // ==========================================
        resultado.Sucesso.Should().BeTrue();
        resultado.Dados.Should().NotBeNull();
        resultado.Dados.EstudanteId.Should().Be(estudanteId);
        resultado.Dados.TurmaId.Should().Be(turmaId);

        // Verificando se os métodos de persistência foram chamados certinho
        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Once);
        _uowMock.Verify(r => r.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Turma_Estiver_Lotada()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var codigoTurma = new CodigoTurma("CSH", 2026, 1, 1);
        var turmaFake = new Turma(codigoTurma, Guid.NewGuid(), Guid.NewGuid(), capacidadeMaxima: 30);
        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync(estudanteFake);
        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaId))
                .ReturnsAsync(turmaFake);
        _uowMock.Setup(r => r.Matriculas.ExisteMatriculaAtivaAsync(estudanteId, turmaId))
                .ReturnsAsync(false);
        // Simulando que a turma já está lotada (30 alunos matriculados)
        _uowMock.Setup(r => r.Matriculas.ContarMatriculasAtivasNaTurmaAsync(turmaId))
                .ReturnsAsync(30);
        var resultado = await _useCase.ExecutarAsync(dto);
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("Turma lotada! Capacidade máxima atingida.");
        // Verificando que o método de adicionar matrícula NÃO foi chamado
        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Estudante_Nao_Existir()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);
        // Simulando que o estudante NÃO existe no banco
        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync((Estudante)null);
        var resultado = await _useCase.ExecutarAsync(dto);
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("Estudante não encontrado.");
        // Verificando que os métodos de persistência NÃO foram chamados
        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Turma_Nao_Existir()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync(estudanteFake);
        // Simulando que a turma NÃO existe no banco
        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaId))
                .ReturnsAsync((Turma)null);
        var resultado = await _useCase.ExecutarAsync(dto);
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("Turma não encontrada.");
        // Verificando que os métodos de persistência NÃO foram chamados
        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Estudante_Ja_Estiver_Matriculado_Na_Turma()
    {
        var estudanteId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var dto = new MatriculaDtoCreate(estudanteId, turmaId);
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var codigoTurma = new CodigoTurma("CSH", 2026, 1, 1);
        var turmaFake = new Turma(codigoTurma, Guid.NewGuid(), Guid.NewGuid(), capacidadeMaxima: 30);
        _uowMock.Setup(r => r.Estudantes.ObterPorIdAsync(estudanteId))
                .ReturnsAsync(estudanteFake);
        _uowMock.Setup(r => r.Turmas.ObterPorIdAsync(turmaId))
                .ReturnsAsync(turmaFake);
        // Simulando que o estudante já está matriculado nessa turma
        _uowMock.Setup(r => r.Matriculas.ExisteMatriculaAtivaAsync(estudanteId, turmaId))
                .ReturnsAsync(true);
        var resultado = await _useCase.ExecutarAsync(dto);
        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Be("Este estudante já está matriculado nesta turma.");
        // Verificando que os métodos de persistência NÃO foram chamados
        _uowMock.Verify(r => r.Matriculas.AdicionarAsync(It.IsAny<Matricula>()), Times.Never);
        _uowMock.Verify(r => r.CommitAsync(), Times.Never);
    }
}