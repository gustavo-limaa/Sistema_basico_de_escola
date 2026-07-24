using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Teste.Unit.TestModeloBase;

public class MatriculaTest
{
    private Matricula CriaMatricula()
    { return new Matricula(Guid.NewGuid(), Guid.NewGuid()); }

    [Fact]
    public void CriarMatricula_DeveCriarComSucesso()
    {   // Act
        var matricula = CriaMatricula();
        // Assert
        Assert.NotNull(matricula);
        Assert.Equal(matricula.Id, matricula.Id);
        Assert.Equal(matricula.EstudanteId, matricula.EstudanteId);
    }

    [Fact]
    public void Matricula_DeveTerIdValido()
    {
        var matricula = CriaMatricula();
        Assert.NotEqual(Guid.Empty, matricula.Id);
    }

    [Fact]
    public void Matricula_DeveTerEstudanteIdValido()
    {
        var matricula = CriaMatricula();
        Assert.NotEqual(Guid.Empty, matricula.EstudanteId);
    }

    [Fact]
    public void Matricula_DeveTerTurmaIdValido()
    {
        var matricula = CriaMatricula();
        Assert.NotEqual(Guid.Empty, matricula.TurmaId);
    }

    [Fact]
    public void Matricula_DeveSerAtivaAoCriar()
    {
        var matricula = CriaMatricula();
        Assert.True(matricula.Ativo);
    }

    [Fact]
    public void Matricula_DeveDesativarCorretamente()
    {
        var matricula = CriaMatricula();
        matricula.Desativar();
        Assert.False(matricula.Ativo);
    }

    [Fact]
    public void Matricula_DeveAtivarCorretamente()
    {
        var matricula = CriaMatricula();
        matricula.Desativar();
        matricula.ativar();
        Assert.True(matricula.Ativo);
    }
}