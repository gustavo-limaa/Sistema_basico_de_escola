using Bogus;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;

namespace SistemaDeMatricula.Teste.Unit.TestModeloBase;

public class TurmaTest
{
    private static Faker<Turma> TurmaFaker(Guid? professorId = null, Guid? disciplinaId = null, int? capacidadeForçada = null)
        => new Faker<Turma>("pt_BR")
        .CustomInstantiator(f =>
        {
            var profId = professorId ?? Guid.NewGuid();
            var discId = disciplinaId ?? Guid.NewGuid();

            // Se 'capacidadeForçada' tiver valor (vinda do teste), usa ela.
            // Se for null, sorteia o aleatório (mantém compatibilidade com outros testes).
            var capacidade = capacidadeForçada ?? f.Random.Int(10, 100);

            var codigo = new CodigoTurma(
                sigla: f.Random.AlphaNumeric(3).ToUpper(),
                ano: f.Date.Soon().Year,
                semestre: f.Random.Int(1, 2),
                numero: f.Random.Int(1, 999)
            );

            return new Turma(codigo, profId, discId, capacidade);
        });

    [Fact]
    public void CriarTurma_Valida()
    {
        // Arrange
        var professorId = Guid.NewGuid();
        var disciplinaId = Guid.NewGuid();
        var capacidade = 30;
        // Act
        var turma = TurmaFaker(professorId, disciplinaId, capacidade).Generate();
        // Assert
        Assert.NotNull(turma);
        Assert.Equal(professorId, turma.ProfessorId);
        Assert.Equal(disciplinaId, turma.DisciplinaId);
        Assert.Equal(capacidade, turma.CapacidadeMaxima);
    }

    [Fact]
    public void CriarTurma_CapacidadeInvalida_DeveLancarExcecao()
    {
        // Arrange
        var professorId = Guid.NewGuid();
        var disciplinaId = Guid.NewGuid();
        var capacidadeInvalida = 0; // Capacidade deve ser maior que 0
        // Act & Assert
        Assert.Throws<DomainException>(() => TurmaFaker(professorId, disciplinaId, capacidadeInvalida).Generate());
    }

    [Fact]
    public void CriarTurma_ProfessorIdInvalido_DeveLancarExcecao()
    {
        // Arrange
        var disciplinaId = Guid.NewGuid();
        var capacidade = 30;
        var professorIdInvalido = Guid.Empty; // Guid vazio é considerado inválido
        // Act & Assert
        Assert.Throws<DomainException>(() => TurmaFaker(professorIdInvalido, disciplinaId, capacidade).Generate());
    }

    [Fact]
    public void CriarTurma_DisciplinaIdInvalido_DeveLancarExcecao()
    {
        // Arrange
        var professorId = Guid.NewGuid();
        var capacidade = 30;
        var disciplinaIdInvalido = Guid.Empty; // Guid vazio é considerado inválido
        // Act & Assert
        Assert.Throws<DomainException>(() => TurmaFaker(professorId, disciplinaIdInvalido, capacidade).Generate());
    }

    [Fact]
    public void CriarTurma_CapacidadeExcessiva_DeveLancarExcecao()
    {
        // Arrange
        var professorId = Guid.NewGuid();
        var disciplinaId = Guid.NewGuid();
        var capacidadeExcessiva = 1000; // Capacidade máxima deve ser razoável (ex: até 500)
        // Act & Assert
        Assert.Throws<DomainException>(() => TurmaFaker(professorId, disciplinaId, capacidadeExcessiva).Generate());
    }

    [Fact]
    public void TemVagaDisponivel_DeveRetornarTrueQuandoHaVagas()
    {
        // Arrange
        var turma = TurmaFaker().Generate();
        var totalMatriculados = turma.CapacidadeMaxima - 1; // Deixa 1 vaga disponível
        // Act
        var resultado = turma.TemVagaDisponivel(totalMatriculados);
        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void TemVagaDisponivel_DeveRetornarFalseQuandoNaoHaVagas()
    {
        // Arrange
        var turma = TurmaFaker().Generate();
        var totalMatriculados = turma.CapacidadeMaxima; // Não deixa nenhuma vaga disponível
        // Act
        var resultado = turma.TemVagaDisponivel(totalMatriculados);
        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void TemVagaDisponivel_DeveRetornarFalseQuandoTotalMatriculadosExcedeCapacidade()
    {
        // Arrange
        var turma = TurmaFaker().Generate();
        var totalMatriculados = turma.CapacidadeMaxima + 1; // Excede a capacidade
        // Act
        var resultado = turma.TemVagaDisponivel(totalMatriculados);
        // Assert
        Assert.False(resultado);
    }
}