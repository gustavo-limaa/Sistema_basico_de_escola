using SistemaDeMatricula.Domain.Value_Object;

namespace SistemaDeMatricula.Domain.Modelos;

public sealed class Turma : ModeloMain
{
    public CodigoTurma CodigoTurma { get; private set; }

    public Guid ProfessorId { get; private set; }

    public Professor Professor { get; private set; }

    public Guid DisciplinaId { get; private set; }
    public Disciplina Disciplina { get; private set; }

    public List<Matricula> Matriculas { get; private set; } = new();

    public Turma(CodigoTurma codigo, Guid professorId, Guid disciplinaId) : base()
    {
        if (string.IsNullOrWhiteSpace(codigo.ValorFormatado)) throw new ArgumentException("Código da turma é obrigatório.");

        CodigoTurma = codigo;
        ProfessorId = professorId;
        DisciplinaId = disciplinaId;
    }

    protected Turma()
    { }

    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    public void AtualizarDados(CodigoTurma novoCodigo, Guid novoProfessorId, Guid novaDisciplinaId)
    {
        if (string.IsNullOrWhiteSpace(novoCodigo.ValorFormatado))
            throw new ArgumentException("Código inválido.");

        if (novoProfessorId == Guid.Empty || novaDisciplinaId == Guid.Empty)
            throw new ArgumentException("Professor e Disciplina são obrigatórios.");

        CodigoTurma = novoCodigo;
        ProfessorId = novoProfessorId;
        DisciplinaId = novaDisciplinaId;
    }
}