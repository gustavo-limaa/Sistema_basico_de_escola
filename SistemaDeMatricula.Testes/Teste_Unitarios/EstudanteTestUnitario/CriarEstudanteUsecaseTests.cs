using Moq;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Aplicacao.Usecases.Estudante;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.EstudanteTestUnitario;

public class CriarEstudanteUsecaseTests
{
    private readonly Mock<IRepositorioEstudante> _repositorioMock;
    private readonly UsesCasesCriarEstudante _useCase;

    public CriarEstudanteUsecaseTests()
    {
        // 1. Criamos o dublê do repositório
        _repositorioMock = new Mock<IRepositorioEstudante>();

        // 2. Injetamos o dublê no Use Case
        _useCase = new UsesCasesCriarEstudante(_repositorioMock.Object);
    }

    [Fact]
    public async Task Executar_DeveRetornarErro_QuandoCpfJaExistir()
    {
        // Arrange
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dto = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        // CONFIGURAÇÃO DO MOCK: Aqui a gente simula que o CPF JÁ EXISTE
        _repositorioMock.Setup(r => r.ExisteCpfAsync(dto.Cpf))
                        .ReturnsAsync(true);

        // Act

        var resultado = await _useCase.ExecuteAsync(dto);

        // Assert
        Assert.False(resultado.Sucesso); // ✅ Correto, valida que falhou

        // ❌ Antes: Assert.Contains("CPF já cadastrado", resultado.Mensagem[0].ToString());
        // ✅ Agora:
        Assert.Equal("CPF já cadastrado.", resultado.Mensagem); // Ajuste para como você retorna o erro

        // O MAIS IMPORTANTE: Garantir que o Adicionar NUNCA foi chamado
        _repositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Estudante>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Retonar_Susesso_Quando_Criar_Estudante_Com_Sucesso()
    {
        // Arrange
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dto = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        // CONFIGURAÇÃO DO MOCK: Simula que o CPF NÃO EXISTE
        _repositorioMock.Setup(r => r.ExisteCpfAsync(dto.Cpf))
                        .ReturnsAsync(false);
        // CONFIGURAÇÃO DO MOCK: Simula que o salvamento no banco foi bem-sucedido
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(true);
        // Act
        var resultado = await _useCase.ExecuteAsync(dto);
        // Assert
        Assert.True(resultado.Sucesso); // ✅ Correto, valida que foi sucesso
        // Verifica se o método AdicionarAsync foi chamado exatamente 1 vez com um Estudante que tem o mesmo CPF do DTO
        _repositorioMock.Verify(r => r.AdicionarAsync(It.Is<Estudante>(e => e.Cpf.Valor == dto.Cpf)), Times.Once);
    }

    [Fact]
    public async Task Deve_Retonar_Falha_Quando_Houver_Erro_Interno()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dto = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        _repositorioMock.Setup(r => r.ExisteCpfAsync(dto.Cpf))
                        .ReturnsAsync(false);
        // Simula uma exceção ao tentar salvar no banco
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ThrowsAsync(new Exception("Erro de banco de dados"));
        var resultado = await _useCase.ExecuteAsync(dto);
        Assert.False(resultado.Sucesso);
        Assert.Contains("Erro ao criar estudante: Erro de banco de dados", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retonar_Falha_Quando_Dto_For_Nulo()
    {
        var resultado = await _useCase.ExecuteAsync(null!);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Dados de estudante são obrigatórios.", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retonar_Falha_Quando_SalvarNoBanco_Falhar()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dto = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        _repositorioMock.Setup(r => r.ExisteCpfAsync(dto.Cpf))
                        .ReturnsAsync(false);
        // Simula que o salvamento no banco falhou (retorna false)
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(false);
        var resultado = await _useCase.ExecuteAsync(dto);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Falha ao salvar no banco de dados.", resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retonar_Falha_Quando_Cpf_Ja_Existe()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dto = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.ToString(),
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        _repositorioMock.Setup(r => r.ExisteCpfAsync(dto.Cpf))
                        .ReturnsAsync(true);
        var resultado = await _useCase.ExecuteAsync(dto);
        Assert.False(resultado.Sucesso);
        Assert.Equal("CPF já cadastrado.", resultado.Mensagem);
    }
}