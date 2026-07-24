using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Services;
using SistemaDeMatricula.Test.Shared;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.EstudanteTestUnitario;

public class CriarEstudanteUsecaseTests
{
    private readonly Mock<IRepositorioEstudante> _repositorioMock;
    private readonly UsesCasesCriarEstudante _useCase;
    private readonly Mock<IUsuarioLogadoService> _usuarioLogadoServiceMock;

    public CriarEstudanteUsecaseTests()
    {
        // 1. Criamos o dublê do repositório
        _repositorioMock = new Mock<IRepositorioEstudante>();
        _usuarioLogadoServiceMock = new Mock<IUsuarioLogadoService>();

        // 🎯 Opcional, mas Sênior: Ensinamos o dublê a devolver um ID falso quando for chamado!
        _usuarioLogadoServiceMock.Setup(x => x.ObterUsuarioId()).Returns("id-falso-de-teste-123");

        // 2. Injetamos o dublê no Use Case
        _useCase = new UsesCasesCriarEstudante(_repositorioMock.Object, _usuarioLogadoServiceMock.Object);
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Criar_Estudante_Com_Sucesso()
    {
        // Arrange: Direto, limpo e sem 'new' acoplado!
        var dto = Data_Factory.EstudanteFakerdto.Generate();

        _repositorioMock.Setup(r => r.ExisteCpfAsync(dto.Cpf))
                        .ReturnsAsync(false);

        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(true);

        // Act
        var resultado = await _useCase.ExecuteAsync(dto);

        // Assert
        Assert.True(resultado.Sucesso);
        _repositorioMock.Verify(r => r.AdicionarAsync(It.Is<Estudante>(e => e.Cpf.Valor == dto.Cpf)), Times.Once);
    }

    [Fact]
    public async Task Deve_Retonar_Falha_Quando_Houver_Erro_Interno()
    {
        var estudanteFake = Data_Factory.EstudanteFakerdto.Generate();

        _repositorioMock.Setup(r => r.ExisteCpfAsync(estudanteFake.Cpf))
                        .ReturnsAsync(false);
        // Simula uma exceção ao tentar salvar no banco
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ThrowsAsync(new Exception(MensagensEstudante.ErroBanco));
        var resultado = await _useCase.ExecuteAsync(estudanteFake);
        Assert.False(resultado.Sucesso);
        Assert.Contains(MensagensEstudante.ErroBanco, resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retonar_Falha_Quando_SalvarNoBanco_Falhar()
    {
        var estudanteFake = Data_Factory.EstudanteFakerdto.Generate();
        _repositorioMock.Setup(r => r.ExisteCpfAsync(estudanteFake.Cpf))
                        .ReturnsAsync(false);
        // Simula que o salvamento no banco falhou (retorna false)
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync())
                        .ReturnsAsync(false);
        var resultado = await _useCase.ExecuteAsync(estudanteFake);
        Assert.False(resultado.Sucesso);
        Assert.Equal(MensagensEstudante.ErroBanco, resultado.Mensagem);
    }

    [Fact]
    public async Task Deve_Retonar_Falha_Quando_Cpf_Ja_Existe()
    {
        var estudanteFake = Data_Factory.EstudanteFakerdto.Generate();
        var estudnate = Data_Factory.EstudanteFaker.Generate();

        _repositorioMock.Setup(r => r.ObterPorCpfAsync(estudanteFake.Cpf))
                        .ReturnsAsync(estudnate);
        var resultado = await _useCase.ExecuteAsync(estudanteFake);
        Assert.False(resultado.Sucesso);
        Assert.Equal(MensagensEstudante.ErroDeDuplicidade, resultado.Mensagem);
    }
}