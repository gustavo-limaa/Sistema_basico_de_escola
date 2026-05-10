using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.TurmaTestUnitario;

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
    public async Task Criar_Turma_Deve_Retornar_Ok_Quando_Dados_Sao_Validos()
    {
        // Arrange
        var dto = CriarTUrma(); // Usa o seu método auxiliar que já gera o DTO com Bogus

        // Simulando que o Professor existe e está ativo
        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar(); // Garante que está ativo
        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        // Simulando que a Disciplina existe e está ativa
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);

        // Simulando que NÃO existe outra turma com o mesmo código
        _mockTurma.Setup(t => t.ObterPorCodigoAsync(It.IsAny<string>())).ReturnsAsync((Turma)null);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Once);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Falha_Quando_Professor_Nao_Existe()
    {
        // Arrange
        var dto = CriarTUrma();

        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId))
                 .ReturnsAsync((Professor)null);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();

        // DICA: Além de ser falso, verifique se a mensagem é a correta
        resultado.Mensagem.Should().Be("Professor não encontrado.");
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Falha_Quando_Disciplina_Nao_Existe()
    {
        // Arrange
        var dto1 = CriarTUrma();

        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar(); // Garante que está ativo
        _mockprof.Setup(p => p.ObterPorIdAsync(dto1.ProfessorId)).ReturnsAsync(professorFake);

        _mockdisc.Setup(d => d.ObterPorIdAsync(dto1.DisciplinaId))
                 .ReturnsAsync((Disciplina)null);

        _mockTurma.Setup(t => t.ObterPorCodigoAsync(It.IsAny<string>())).ReturnsAsync((Turma)null);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto1);

        // Assert
        resultado.Sucesso.Should().BeFalse();

        // DICA: Além de ser falso, verifique se a mensagem é a correta
        resultado.Mensagem.Should().Be("Disciplina não encontrada.");
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Conflito_Quando_Professor_inativo()
    {
        // Arrange
        var dto = CriarTUrma();

        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Desativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        // DICA: Além de ser falso, verifique se a mensagem é a correta
        resultado.Mensagem.Should().Be("Não é possível vincular um Professor inativo a uma nova turma.");
        resultado.Tipo.Should().Be(TipoErro.Conflito);
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Conflito_Quando_Disciplina_inativa()
    {
        // Arrange
        var dto = CriarTUrma();

        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        disciplinaFake.Desativar();
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();

        // DICA: Além de ser falso, verifique se a mensagem é a correta
        resultado.Mensagem.Should().Be("Não é possível vincular uma disciplina inativa a uma nova turma.");
        resultado.Tipo.Should().Be(TipoErro.Conflito);
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Retornar_Conflito_Quando_Turma_Tiver_Mesmo_Codigo()
    {
        // Arrange
        var dto = CriarTUrma();

        // 1. Setup do Professor (Caminho livre)
        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        // 2. Setup da Disciplina (Caminho livre)
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);

        // 3. O PULO DO GATO: Simular que já existe uma turma com esse código
        // Criamos uma instância de turma qualquer para o Mock retornar
        var turmaExistente = DataFactory.TurmaFaker().Generate();

        // Configuramos o Mock para retornar essa turma em vez de null
        _mockTurma.Setup(t => t.ObterPorCodigoAsync(It.IsAny<string>()))
                  .ReturnsAsync(turmaExistente);

        // Act
        var resultado = await _usecase.ExecutarAsync(dto);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoErro.Conflito); // Se você tiver o enum TipoErro
        resultado.Mensagem.Should().Be("Já existe uma turma ativa com este código.");

        // Verificação de segurança: não pode ter chamado o Adicionar
        _mockTurma.Verify(t => t.AdicionarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Criar_Turma_Deve_Lancar_Excecao_Quando_Banco_De_Dados_Falhar()
    {
        // Arrange
        var dto = CriarTUrma();
        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        // 2. Setup da Disciplina (Caminho livre)
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);

        _mockTurma.Setup(t => t.ObterPorCodigoAsync(It.IsAny<string>())).ReturnsAsync((Turma)null);

        // O PULO DO GATO: Forçamos o repositório a lançar uma exceção de banco
        _mockTurma.Setup(t => t.AdicionarAsync(It.IsAny<Turma>()))
                  .ThrowsAsync(new Exception("Erro de conexão com o banco de dados"));

        // Act
        // Usamos o Func para capturar a exceção no Act
        Func<Task> acao = async () => await _usecase.ExecutarAsync(dto);

        // Assert
        await acao.Should().ThrowAsync<Exception>()
                  .WithMessage("Erro de conexão com o banco de dados");
    }

    [Fact]
    public async Task Criar_Turma_Deve_Lancar_Excecao_Quando_Banco_Falhar()
    {
        // Arrange
        var dto = CriarTUrma(); // Seu método auxiliar

        // Caminho livre até o banco
        // 1. Setup do Professor (Caminho livre)
        var professorFake = DataFactory.ProfessorFaker.Generate();
        professorFake.Ativar();
        _mockprof.Setup(p => p.ObterPorIdAsync(dto.ProfessorId)).ReturnsAsync(professorFake);

        // 2. Setup da Disciplina (Caminho livre)
        var disciplinaFake = DataFactory.DisciplinaFaker.Generate();
        disciplinaFake.Ativar();
        _mockdisc.Setup(d => d.ObterPorIdAsync(dto.DisciplinaId)).ReturnsAsync(disciplinaFake);
        // FORÇANDO O ERRO: O banco explode aqui
        _mockTurma.Setup(t => t.AdicionarAsync(It.IsAny<Turma>()))
                  .ThrowsAsync(new Exception("Erro fatal no MySQL"));

        // Act
        var agir = async () => await _usecase.ExecutarAsync(dto);

        // Assert
        await agir.Should().ThrowAsync<Exception>()
                  .WithMessage("Erro fatal no MySQL");
    }
}