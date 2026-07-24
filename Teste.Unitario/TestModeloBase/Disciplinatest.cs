using Bogus;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;

namespace SistemaDeMatricula.Teste.Unit.TestModeloBase;

public class Disciplinatest
{
    public static Faker<Disciplina> DisciplinaFaker => new Faker<Disciplina>()
        .CustomInstantiator(f =>
        {
            var materias = new[] { "Matemática", "Cálculo", "Algoritmos", "Banco de Dados", "História" };
            var nomeSorteado = f.PickRandom(materias) + " " + f.Random.Replace("##");

            return new Disciplina(
                nome: nomeSorteado,
                cargaHoraria: f.Random.Int(30, 120)
            );
        });

    [Fact]
    public void CriarDisciplina_Valida()
    {
        // Arrange
        var disciplina = DisciplinaFaker.Generate();
        // Act & Assert
        Assert.NotNull(disciplina);
        Assert.False(string.IsNullOrWhiteSpace(disciplina.Nome));
        Assert.InRange(disciplina.CargaHoraria.Valor, 30, 120);
    }

    [Fact]
    public void CriarDisciplina_NomeInvalido_DeveLancarExcecao()
    {
        // Arrange
        var nomeInvalido = "   "; // Nome vazio ou apenas espaços
        var cargaHorariaValida = 60;
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new Disciplina(nomeInvalido, cargaHorariaValida));
        Assert.Equal("O nome da disciplina é obrigatório.", exception.Message);
    }

    [Fact]
    public void CriarDisciplina_CargaHorariaInvalida_DeveLancarExcecao()
    {
        // Arrange
        var nomeValido = "Matemática Avançada";
        var cargaHorariaInvalida = -10; // Carga horária negativa
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new Disciplina(nomeValido, cargaHorariaInvalida));
        Assert.Equal("A carga horária deve ser positiva.", exception.Message);
    }

    [Fact]
    public void AtualizarDisciplina_Valida()
    {
        // Arrange
        var disciplina = DisciplinaFaker.Generate();
        var novoNome = "Física Moderna";
        var novaCargaHoraria = 80;
        // Act
        disciplina.AtualizarDados(novoNome, novaCargaHoraria);
        // Assert
        Assert.Equal(novoNome, disciplina.Nome);
        Assert.Equal(novaCargaHoraria, disciplina.CargaHoraria.Valor);
    }

    [Fact]
    public void AtualizarDisciplina_NomeInvalido_DeveLancarExcecao()
    {
        // Arrange
        var disciplina = DisciplinaFaker.Generate();
        var nomeInvalido = "   "; // Nome vazio ou apenas espaços
        var novaCargaHoraria = 80;
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => disciplina.AtualizarDados(nomeInvalido, novaCargaHoraria));
        Assert.Equal("O nome da disciplina é obrigatório.", exception.Message);
    }

    [Fact]
    public void AtualizarDisciplina_CargaHorariaInvalida_DeveLancarExcecao()
    {
        // Arrange
        var disciplina = DisciplinaFaker.Generate();
        var nomeValido = "Física Moderna";
        var cargaHorariaInvalida = -10; // Carga horária negativa
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => disciplina.AtualizarDados(nomeValido, cargaHorariaInvalida));
        Assert.Equal("A carga horária deve ser positiva.", exception.Message);
    }

    [Fact]
    public void AtivarDesativarDisciplina()
    {
        // Arrange
        var disciplina = DisciplinaFaker.Generate();
        // Act
        disciplina.Desativar();
        // Assert
        Assert.False(disciplina.Ativo);
        // Act
        disciplina.Ativar();
        // Assert
        Assert.True(disciplina.Ativo);
    }
}