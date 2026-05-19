using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.ProfessorTestUnitario;

public class PegarTudoEPegarPorIdProfessorTestUnitario
{
    private readonly Mock<IUnitOfWork> _Uow;

    private readonly Mock<IRepositorioProfessor
        > _profe;

    private readonly ProfessorObterPorIdUsecases _caseID;
    private readonly ProfessorObterPorCpfUsecases _caseCpf;
    private readonly ProfessorObterTodosUsecases _caseTodos;

    public PegarTudoEPegarPorIdProfessorTestUnitario()
    {
        _Uow = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };
        _profe = new Mock<IRepositorioProfessor> { DefaultValue = DefaultValue.Mock };
        _caseID = new ProfessorObterPorIdUsecases(_profe.Object);
        _caseCpf = new ProfessorObterPorCpfUsecases(_profe.Object);
        _caseTodos = new ProfessorObterTodosUsecases(_profe.Object);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarProfessor()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        _profe.Setup(r => r.ObterPorIdAsync(professor.Id)).ReturnsAsync(professor);
        // Act
        var resultado = await _caseID.ExecutarAsync
            (professor.Id);
        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(professor.Id, resultado.Dados.ProfessorId);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaDeProfessores()
    {
        // Arrange
        var professores = DataFactory.ProfessorFaker.Generate(5);
        _profe.Setup(r => r.ObterTodosAsync()).ReturnsAsync(professores);
        // Act
        var resultado = await _caseTodos.ExecutarAsync();
        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.Dados.Count); // Propriedade pura, super rápida;
    }

    [Fact]
    public async Task ObterPorCpfAsync_DeveRetornarProfessor()
    {
        // Arrange
        var professor = DataFactory.ProfessorFaker.Generate();
        _profe.Setup(r => r.ObterPorCpfAsync(professor.Cpf.Valor)).ReturnsAsync(professor);
        // Act
        var resultado = await _caseCpf.ExecutarAsync(professor.Cpf.Valor);
        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(professor.Cpf.Valor, resultado.Dados.Cpf);
    }

    [Fact]
    public async Task ObterPorCpfAsync_ComCpfInvalido_DeveRetornarFalha()
    {
        // Arrange
        string cpfInvalido = "123.456.789-00";
        // Act
        var resultado = await _caseCpf.ExecutarAsync(cpfInvalido);
        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComIdInvalido_DeveRetornarFalha()
    {
        // Arrange
        Guid idInvalido = Guid.Empty;
        // Act
        var resultado = await _caseID.ExecutarAsync(idInvalido);
        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComProfessorNaoEncontrado_DeveRetornarNaoEncontrado()
    {
        // Arrange
        Guid idInexistente = Guid.NewGuid();
        _profe.Setup(r => r.ObterPorIdAsync(idInexistente)).ReturnsAsync((Professor?)null);
        // Act
        var resultado = await _caseID.ExecutarAsync(idInexistente);
        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Professor não encontrado.", resultado.Mensagem);
    }
}