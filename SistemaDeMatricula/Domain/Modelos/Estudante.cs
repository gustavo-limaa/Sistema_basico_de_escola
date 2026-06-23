using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeMatricula.Domain.Modelos;

public sealed class Estudante : ModeloMain
{
    public Estudante(Guid estudanteId, ObjectNomeCompleto nomeCompleto, ObjectDataNascimento dataNascimento, ObjectCPF cpf, ObjectEmail email, ObjectTelefone telefone) : base()
    {
        if (estudanteId == Guid.Empty) throw new DomainException("O ID do estudante não pode ser vazio.");

        NomeCompleto = nomeCompleto;
        DataNascimento = dataNascimento;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
    }

    public Estudante()
    { }

    public string UsuarioId { get; private set; } = Guid.NewGuid().ToString();

    public ObjectNomeCompleto NomeCompleto { get; private set; }

    public ObjectDataNascimento DataNascimento { get; private set; }

    public ObjectCPF Cpf { get; private set; }

    public ObjectEmail Email { get; private set; }

    public ObjectTelefone Telefone { get; private set; }

    private readonly List<Matricula> _matriculas = new();
    public IReadOnlyCollection<Matricula> Matriculas => _matriculas.AsReadOnly();

    public void AtualizarDados(ObjectNomeCompleto nome, ObjectEmail email, ObjectDataNascimento data, ObjectTelefone telefone)
    {
        this.NomeCompleto = nome;
        this.Email = email;
        this.DataNascimento = data;
        this.Telefone = telefone;
    }

    public void AdicionarMatricula(Matricula matricula)

    {
        if (_matriculas.Any(m => m.Id == matricula.Id))
            throw new DomainException("Já existe uma matrícula com este ID para este estudante.");
        if (_matriculas.Any(m => m.TurmaId == matricula.TurmaId))
            throw new DomainException("O estudante já está matriculado nesta turma.");
        // Verifica se já existe matrícula com a mesma turma E já aprovada
        if (_matriculas.Any(m => m.TurmaId == matricula.TurmaId && m.Aprovado))
            throw new DomainException("O estudante já foi aprovado nesta turma.");
        // Verifica recuperação apenas para a mesma turma
        if (_matriculas.Any(m => m.TurmaId == matricula.TurmaId && m.Recuperacao))
            throw new DomainException("O estudante está em recuperação nesta turma.");

        _matriculas.Add(matricula);
    }

    public void RemoverMatricula(Matricula matricula)
    {
        if (!_matriculas.Contains(matricula))
            throw new DomainException("Esta matrícula não pertence a este estudante.");

        _matriculas.Remove(matricula);
    }

    public void VincularUsuario(string usuarioIdDoToken)
    {
        UsuarioId = usuarioIdDoToken;
    }
}