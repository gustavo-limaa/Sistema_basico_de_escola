using FluentAssertions; // Recomendo muito usar para deixar o código legível
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using Xunit;

namespace SistemaDeMatricula.Teste.Unit.TestModeloBase
{
    public class NotaTests
    {
        // Este é um método auxiliar que prepara o objeto para você
        private Nota CriarNotaValida(
            double valor = 5.0,
            string descricao = "Descrição padrão")
        {
            // O construtor da sua entidade deve conter a lógica de validação
            return new Nota(Guid.NewGuid(), TipoImportancia.Media, CategoriaAvaliacao.Prova, valor, descricao, DateTime.Now);
        }

        [Fact]
        public void Deve_Criar_Nota_Com_Valores_Validos()
        {
            // Arrange & Act
            var nota = CriarNotaValida(8.5, "Prova de C#");

            // Assert
            nota.Valor.Should().Be(8.5);
            nota.Descricao.Should().Be("Prova de C#");
        }

        [Theory] // Usamos Theory para testar vários valores inválidos de uma vez
        [InlineData(-1.0)]
        [InlineData(11.0)]
        public void Nao_Deve_Criar_Nota_Com_Valor_Fora_Do_Limite(double valorInvalido)
        {
            // Act
            Action act = () => CriarNotaValida(valor: valorInvalido);

            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("Nota não pode ser negativa É nem menor q 0 ou maior que 10");
        }

        [Fact]
        public void Nao_Deve_Criar_Nota_Com_Descricao_Vazia()
        {
            // Act
            Action act = () => CriarNotaValida(descricao: "");

            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("Descrição não pode ser vazia");
        }

        [Fact]
        public void Nao_Deve_Criar_Nota_Com_Descricao_Nula()
        {
            // Act
            Action act = () => CriarNotaValida(descricao: null);
            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("Descrição não pode ser vazia");
        }

        [Fact]
        public void Deve_Atualizar_Dados_Validados()
        {
            // Arrange
            var nota = CriarNotaValida();
            // Act
            nota.AtualizarDados(9.0, "Prova de .NET", TipoImportancia.Alta, CategoriaAvaliacao.Prova);
            // Assert
            nota.Valor.Should().Be(9.0);
            nota.Descricao.Should().Be("Prova de .NET");
            nota.Importancia.Should().Be(TipoImportancia.Alta);
            nota.Categoria.Should().Be(CategoriaAvaliacao.Prova);
        }

        [Fact]
        public void Nao_Deve_Atualizar_Dados_Com_Categoria_Invalida()
        {
            // Arrange
            var nota = CriarNotaValida();
            // Act
            Action act = () => nota.AtualizarDados(8.0, "Descrição", TipoImportancia.Media, (CategoriaAvaliacao)999);
            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("Categoria de avaliação inválida");
        }

        [Fact]
        public void Nao_Deve_Atualizar_Dados_Com_Importancia_Invalida()
        {
            // Arrange
            var nota = CriarNotaValida();
            // Act
            Action act = () => nota.AtualizarDados(8.0, "Descrição", (TipoImportancia)999, CategoriaAvaliacao.Prova);
            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("Tipo de importância inválido");
        }
    }
}