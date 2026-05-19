using FluentAssertions;
using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.ProfessorTestUnitario;

public class AtualizarProfessorTestUnitario
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IRepositorioProfessor> _repositorioProfessor;
    private readonly ProfessorAtualizarUsecase _professor;

    public AtualizarProfessorTestUnitario()
    {
        _uow = new Mock<IUnitOfWork>();
        _repositorioProfessor = new Mock<IRepositorioProfessor>();
        _professor = new ProfessorAtualizarUsecase(_repositorioProfessor.Object);
    }

    [Fact]
    public async Task AtualizarProfessor_Deve_Atualizar_Professor_Cadastrado()
    {
        var professor = DataFactory.ProfessorFaker.Generate();
        _repositorioProfessor.Setup(x => x.ObterPorIdAsync
        (professor.Id)).ReturnsAsync(professor);

        var professorAtualizado = new ProfessorDtoUpdate
            (
             ProfessorId: professor.Id,
             NomeCompleto: "Jane Doe",
                DataNascimento: DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                Email: "jane.doe@example.com",
                Telefone: "(11) 99999-9999",
                Salario: 5000.00m,
                    Categoria: "Titular"
            );

        _repositorioProfessor.Setup(x => x.Atualizar(professor)).Verifiable();
        _repositorioProfessor.Setup(x => x.SalvarAlteracoesAsync()).ReturnsAsync(true);
        // Act
        var result = await _professor.ExecutarAsync(professorAtualizado);

        // Assert
        result.Sucesso.Should().BeTrue();

        _repositorioProfessor.Verify(x => x.Atualizar(professor), Times.Once);
    }

    [Fact]
    public async Task AtualizarProfessor_Deve_Retornar_Erro_Quando_Professor_Nao_Existir()
    {
        var professorId = Guid.NewGuid();
        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professorId)).ReturnsAsync((Professor)null);
        var professorAtualizado = new ProfessorDtoUpdate
            (
             ProfessorId: professorId,
             NomeCompleto: "Jane Doe",
                DataNascimento: DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                Email: "jane.doe@example.com"
                , Telefone: "(11) 99999-9999",
                Salario: 5000.00m,
                    Categoria: "Titular"
                    );
        // Act
        var result = await _professor.ExecutarAsync(professorAtualizado);
        Assert.NotNull(result);
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be("Professor não encontrado.");
    }

    [Fact]
    public async Task AtualizarProfessor_Deve_Retornar_Erro_Quando_Salvar_Alteracoes_Falhar()
    {
        var professor = DataFactory.ProfessorFaker.Generate();
        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professor.Id)).ReturnsAsync(professor);
        var professorAtualizado = new ProfessorDtoUpdate
            (
             ProfessorId: professor.Id,
             NomeCompleto: "Jane Doe",
                DataNascimento: DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                Email: "jane.doe@example.com",
                Telefone: "(11) 99999-9999",

                Salario:
                    5000.00m,
                    Categoria: "Titular"
                    );
        _repositorioProfessor.Setup(x => x.Atualizar(professor)).Verifiable();
        _repositorioProfessor.Setup(x => x.SalvarAlteracoesAsync()).ReturnsAsync(false);
        // Act
        var result = await _professor.ExecutarAsync(professorAtualizado);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AtualizarProfessor_ComEmailJaExistente_DeveRetornarFalha()
    {
        // Arrange
        var professorEditando = DataFactory.ProfessorFaker.Generate();
        var outroProfessor = DataFactory.ProfessorFaker.Generate(); // ID diferente!

        var dtoUpdate = new ProfessorDtoUpdate(professorEditando.Id, "Nome", DateOnly.FromDateTime(DateTime.Now), "email.duplicado@escola.com", "123", 1000, "Titular");

        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professorEditando.Id)).ReturnsAsync(professorEditando);
        _repositorioProfessor.Setup(x => x.ObterPorEmailAsync(dtoUpdate.Email)).ReturnsAsync(outroProfessor);

        // Act
        var result = await _professor.ExecutarAsync(dtoUpdate);

        // Assert
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be("Já existe outro professor cadastrado com este e-mail.");
    }

    [Fact]
    public async Task deve_retornar_erro_quando_dados_do_professor_sao_invalidos()
    {
        // Arrange
        ProfessorDtoUpdate professorDto = null;
        // Act
        var result = await _professor.ExecutarAsync(professorDto);
        // Assert
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be("Dados do professor são inválidos.");
    }

    [Fact]
    public async Task deve_retornar_erro_quando_id_do_professor_for_vazio()
    {
        // Arrange
        var professorDto = new ProfessorDtoUpdate(Guid.Empty, "Nome", DateOnly.FromDateTime(DateTime.Now), "email@example.com", "123", 1000, "Titular");
        // Act
        var result = await _professor.ExecutarAsync(professorDto);
        // Assert
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be("Dados do professor são inválidos.");
    }

    [Fact]
    public async Task deve_retornar_erro_quando_professor_nao_for_encontrado()
    {
        // Arrange
        var professorId = Guid.NewGuid();
        var professorDto = new ProfessorDtoUpdate(professorId, "Nome", DateOnly.FromDateTime(DateTime.Now), " email@example com", "123", 1000, "Titular");
        _repositorioProfessor.Setup(x => x.ObterPorIdAsync(professorId)).ReturnsAsync((Professor)null);
        // Act
        var result = await _professor.ExecutarAsync(professorDto);
        // Assert
        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be("Professor não encontrado.");
    }
}