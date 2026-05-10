using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.ProfessorTestUnitario
{
    public class AddProfessorTest
    {
        private readonly Mock<IRepositorioProfessor> _mockRepositorioProfessor;

        private readonly ProfessorCriarUsecases _professorCriarUsecases;

        public AddProfessorTest()
        {
            _mockRepositorioProfessor = new Mock<IRepositorioProfessor>();
            _professorCriarUsecases = new ProfessorCriarUsecases(_mockRepositorioProfessor.Object);
        }

        [Fact]
        public async Task CriarProfessor_Sucesso()
        {
            // Arrange
            var professor = DataFactory.ProfessorFaker.Generate();

            // FORÇA O SALÁRIO A TER APENAS 2 CASAS DECIMAIS
            var salarioLimpo = Math.Round(professor.Salario.Valor, 2);

            var dto = new ProfessorDtoCreate(
                NomeCompleto: professor.NomeCompleto.Valor,
                Cpf: professor.Cpf.Valor,
                DataNascimento: professor.DataNascimento.Valor,
                Email: professor.Email.Valor,
                Telefone: professor.Telefone.Valor,
                Salario: salarioLimpo, // <--- USA O VALOR LIMPO AQUI
                Categoria: professor.Categoria.ToString()
            );
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
            var professor = DataFactory.ProfessorFaker.Generate();

            var dto = new ProfessorDtoCreate(
                NomeCompleto: professor.NomeCompleto.ToString(),
                Cpf: professor.Cpf.Valor,
                DataNascimento: professor.DataNascimento.Valor,
                Email: professor.Email.Valor,
                Telefone: professor.Telefone.Valor,
                Salario: professor.Salario.Valor,
                Categoria: professor.Categoria.ToString()
            );

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
            var dto = new ProfessorDtoCreate(
                NomeCompleto: "Professor Girafales",
                Cpf: "11144477735", // <--- CPF matematicamente VÁLIDO e fixo!
                DataNascimento: DateOnly.Parse("1980-01-01"),
                Email: "girafales@escola.com",
                Telefone: "(11) 99999-9999",
                Salario: -5000m, // <--- O ÚNICO DADO INVÁLIDO NO DTO
                Categoria: "Titular"
            );

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

            // Bônus: Se quiser garantir que a mensagem de erro foi a do salário, descomente abaixo:
            // Assert.Equal("O salário não pode ser negativo.", resultado.Mensagem);

            // Verificações de Mock
            _mockRepositorioProfessor.Verify(repo => repo.AdicionarAsync(It.IsAny<Professor>()), Times.Never);
            _mockRepositorioProfessor.Verify(repo => repo.SalvarAlteracoesAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarProfessor_Falha_CpfInvalido()
        {
            // Arrange
            var dto = new ProfessorDtoCreate(
                NomeCompleto: "John Doe",
                Cpf: "123.456.789-00", // CPF inválido para testar a validação
                DataNascimento: DateOnly.Parse("1990-01-01"),
                Email: "john.doe@example.com",
                Telefone: "(11) 99999-9999",
                Salario: 5000m,
                Categoria: "Titular"
            );

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
            // Arrange
            var dto = new ProfessorDtoCreate(
                NomeCompleto: "John Doe",
                Cpf: "123.456.789-09", // CPF válido para não interferir na validação de email
                DataNascimento: DateOnly.Parse("1990-01-01"),
                Email: "john.doe@invalid", // Email inválido para testar a validação
                Telefone: "(11) 99999-9999",
                Salario: 5000m,
                Categoria: "Titular"
            );
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