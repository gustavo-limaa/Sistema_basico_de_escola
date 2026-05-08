using SitemaDeMatricula.Domain.Value_Object;

namespace SitemaDeMatricula.Domain.Modelos;

public class Turma
{
    public Guid TurmaId { get; private set; }
    public CodigoTurma CodigoTurma { get; private set; }
    public bool Ativo { get; private set; } // Importante para o Soft Delete

    // Relacionamentos
    public Guid ProfessorId { get; private set; }

    public virtual Professor Professor { get; private set; }

    public Guid DisciplinaId { get; private set; }
    public virtual Disciplina Disciplina { get; private set; }

    public List<Matricula> Matriculas { get; private set; } = new();

    // Construtor Público para criação (Domínio)
    public Turma(CodigoTurma codigo, Guid professorId, Guid disciplinaId)
    {
        // Validação básica: se o código for vazio, o sistema nem deixa criar
        if (string.IsNullOrWhiteSpace(codigo.ValorFormatado)) throw new ArgumentException("Código da turma é obrigatório.");

        TurmaId = Guid.NewGuid();
        CodigoTurma = codigo;
        ProfessorId = professorId;
        DisciplinaId = disciplinaId;
        Ativo = true;
    }

    // Construtor para o Entity Framework (O "fantasma" que o banco usa)
    protected Turma()
    { }

    // Comportamentos
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