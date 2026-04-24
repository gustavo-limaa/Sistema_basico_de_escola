using Moq;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Aplicacao.Usecases.Professor;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                NomeCompleto: professor.NomeCompleto.ToString(),
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
    }
}