using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Services;
using SistemaDeMatricula.Test.Shared;
using SistemaDeMatricula.Teste.Unit.Teste_Unitarios;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.ProfessorTestUnitario
{
    public class AddProfessorTest
    {
        private readonly Mock<IRepositorioProfessor> _mockRepositorioProfessor;
        private readonly Mock<IUsuarioLogadoService> _mockUsuarioLogadoService;

        private readonly ProfessorCriarUsecases _professorCriarUsecases;

        public AddProfessorTest()
        {
            _mockRepositorioProfessor = new Mock<IRepositorioProfessor>();
            _mockUsuarioLogadoService = new Mock<IUsuarioLogadoService>();
            _mockUsuarioLogadoService.Setup(x => x.ObterUsuarioId()).Returns("id-falso-de-teste-123");

            _professorCriarUsecases = new ProfessorCriarUsecases(_mockRepositorioProfessor.Object, _mockUsuarioLogadoService.Object);
        }

        [Fact]
        public async Task CriarProfessor_Sucesso()
        {
            var dto = Data_Factory.ProfessorFakerdto.Generate();
            _mockRepositorioProfessor

                .Setup(repo => repo.AdicionarAsync(It.IsAny<Professor>()))
                .Returns(Task.CompletedTask);

            _mockRepositorioProfessor
                .Setup(repo => repo.SalvarAlteracoesAsync())
                .ReturnsAsync(true);

            _mockRepositorioProfessor
                .Setup(repo => repo.ObterPorCpfAsync(dto.Cpf))
                .ReturnsAsync(default(Professor?));

            // Act
            var resultado = await _professorCriarUsecases.ExecutarAsync(dto);
            // Assert
            Assert.True(resultado.Sucesso);
            Assert.NotNull(resultado.Dados);

            // Comparando Strings (CPF, Nome, Email)
            Assert.Equal(dto.Cpf, resultado.Dados.Cpf);
            Assert.Equal(dto.NomeCompleto, resultado.Dados.NomeCompleto);

            // Comparando Decimais (Salário)
            Assert.Equal(dto.Salario, resultado.Dados.Salario);

            // Verificações de Mock
            _mockRepositorioProfessor.Verify(repo => repo.AdicionarAsync(It.IsAny<Professor>()), Times.Once);
            _mockRepositorioProfessor.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarProfessor_Falha_CpfExistente()
        {
            // Arrange

            var professor = Data_Factory.ProfessorFaker.Generate();

            var dto = Data_Factory.ProfessorFakerdto.Generate();

            _mockRepositorioProfessor
                .Setup(repo => repo.ObterPorCpfAsync(dto.Cpf))
                .ReturnsAsync(professor);
            _mockRepositorioProfessor.Setup(repo => repo.AdicionarAsync(It.IsAny<Professor>()))
                .Returns(Task.CompletedTask);
            _mockRepositorioProfessor.Setup(repo => repo.SalvarAlteracoesAsync())
                .ReturnsAsync(true);

            // Act
            var resultado = await _professorCriarUsecases.ExecutarAsync(dto);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Null(resultado.Dados);

            // Verificações de Mock
            _mockRepositorioProfessor.Verify(repo => repo.AdicionarAsync(It.IsAny<Professor>()), Times.Never);
            _mockRepositorioProfessor.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarProfessor_Falha_SalarioNegativo()
        {
            // Arrange
            var dto = Data_Factory.ProfessorFakerdto.Generate();
            dto = dto with { Salario = -5000m }; // único dado inválido no DTO

            _mockRepositorioProfessor
                .Setup(repo => repo.ObterPorCpfAsync(dto.Cpf))
                .ReturnsAsync(default(Professor?));
            _mockRepositorioProfessor
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Professor>()))
                .Returns(Task.CompletedTask);
            _mockRepositorioProfessor
                .Setup(repo => repo.SalvarAlteracoesAsync())
                .ReturnsAsync(true);

            // Act
            var resultado = await _professorCriarUsecases.ExecutarAsync(dto);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Null(resultado.Dados);

            // Verificações de Mock
            _mockRepositorioProfessor.Verify(repo => repo.AdicionarAsync(It.IsAny<Professor>()), Times.Never);
            _mockRepositorioProfessor.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarProfessor_Falha_CpfInvalido()
        {
            // Arrange
            var dto = Data_Factory.ProfessorFakerdto.Generate();
            dto = dto with
            {
                Cpf = "123.456.789-00" // CPF inválido para testar a validação
            };

            _mockRepositorioProfessor
                .Setup(repo => repo.ObterPorCpfAsync(dto.Cpf))
                .ReturnsAsync(default(Professor?));
            _mockRepositorioProfessor.Setup(repo => repo.AdicionarAsync(It.IsAny<Professor>()))
                .Returns(Task.CompletedTask);
            _mockRepositorioProfessor.Setup(repo => repo.SalvarAlteracoesAsync())
                .ReturnsAsync(true);

            // Act
            var resultado = await _professorCriarUsecases.ExecutarAsync(dto);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Null(resultado.Dados);

            // Verificações de Mock
            _mockRepositorioProfessor.Verify(repo => repo.AdicionarAsync(It.IsAny<Professor>()), Times.Never);
            _mockRepositorioProfessor.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarProfessor_Falha_EmailInvalido()
        {
            var dto = Data_Factory.ProfessorFakerdto.Generate();
            dto = dto with
            {
                Email = "emailinvalido.com" // Email inválido para testar a validação
            };
            _mockRepositorioProfessor
                .Setup(repo => repo.ObterPorCpfAsync(dto.Cpf))
                .ReturnsAsync(default(Professor?));
            _mockRepositorioProfessor
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Professor>()))
                .Returns(Task.CompletedTask);
            _mockRepositorioProfessor
                .Setup(repo => repo.SalvarAlteracoesAsync())
                .ReturnsAsync(true);
            // Act
            var resultado = await _professorCriarUsecases.ExecutarAsync(dto);
            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Null(resultado.Dados);
            // Verificações de Mock
            _mockRepositorioProfessor.Verify(repo => repo.AdicionarAsync(It.IsAny<Professor>()), Times.Never);
            _mockRepositorioProfessor.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Never);
        }
    }
}