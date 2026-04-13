namespace SitemaDeMatricula.Domain.Modelos;

public class Disciplina
{
    public Guid DisciplinaId { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; }
    public int CargaHoraria { get; private set; }

    public ICollection<Turma> Turmas { get; private set; } = new List<Turma>();

    public Disciplina(string nome, int cargaHoraria)
    {
        Nome = nome;
        CargaHoraria = cargaHoraria;
    }

    public Disciplina()
    {
    }
}