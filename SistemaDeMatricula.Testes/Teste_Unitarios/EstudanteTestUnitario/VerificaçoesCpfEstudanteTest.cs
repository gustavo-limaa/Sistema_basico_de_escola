using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.EstudanteTestUnitario
{
    public class VerificaçoesCpfEstudanteTest
    {
        private readonly Mock<IRepositorioEstudante> _mockRepositorioEstudante;
        private readonly UsecaseVerificarCpfEstudante _usecaseVerificaEstudante;

        public VerificaçoesCpfEstudanteTest()
        {
            _mockRepositorioEstudante = new Mock<IRepositorioEstudante>();
            _usecaseVerificaEstudante = new UsecaseVerificarCpfEstudante(_mockRepositorioEstudante.Object);
        }

        [Fact]
        public async Task Deve_Confirmar_Existencia_Quando_Cpf_Estiver_Cadastrado()
        {
            // Arrange
            var cpf = "12345678900";
            _mockRepositorioEstudante.Setup(repo => repo.ExisteCpfAsync(cpf)).ReturnsAsync(true);

            // Act
            var resultado = await _usecaseVerificaEstudante.Executar(cpf);

            // Assert
            Assert.True(resultado.Sucesso);
            Assert.Equal("Estudante Localizado.", resultado.Mensagem); // Combinando com o Use Case
        }

        [Fact]
        public async Task Deve_Retornar_Falha_Quando_Cpf_Nao_Estiver_Cadastrado()
        {
            // Arrange
            var cpf = "12345678900";
            _mockRepositorioEstudante.Setup(repo => repo.ExisteCpfAsync(cpf)).ReturnsAsync(false);

            // Act
            var resultado = await _usecaseVerificaEstudante.Executar(cpf);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("Estudante não encontrado.", resultado.Mensagem);
        }

        [Fact]
        public async Task Deve_Limpar_Cpf_Corretamente()
        {
            // Arrange
            var cpfComFormatacao = "123.456.789-00";
            var cpfLimpo = "12345678900";
            _mockRepositorioEstudante.Setup(repo => repo.ExisteCpfAsync(cpfLimpo)).ReturnsAsync(true);
            // Act
            var resultado = await _usecaseVerificaEstudante.Executar(cpfComFormatacao);
            // Assert
            Assert.True(resultado.Sucesso);
            Assert.Equal("Estudante Localizado.", resultado.Mensagem);
        }

        [Fact]
        public async Task Deve_Lidar_Cpf_Vazio()
        {
            // Arrange
            var cpfVazio = "";
            _mockRepositorioEstudante.Setup(repo => repo.ExisteCpfAsync(cpfVazio)).ReturnsAsync(false);
            // Act
            var resultado = await _usecaseVerificaEstudante.Executar(cpfVazio);
            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("Estudante não encontrado.", resultado.Mensagem);
        }

        [Fact]
        public async Task Deve_Lidar_Cpf_Invalido()
        {
            // Arrange
            var cpfInvalido = "12345678901";
            _mockRepositorioEstudante.Setup(repo => repo.ExisteCpfAsync(cpfInvalido)).ReturnsAsync(false);
            // Act
            var resultado = await _usecaseVerificaEstudante.Executar(cpfInvalido);
            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("Estudante não encontrado.", resultado.Mensagem);
        }
    }
}