using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeMatricula.Domain.Modelos;

public sealed class Estudante : ModeloMain
{
    public Estudante(Guid estudanteId, ObjectNomeCompleto nomeCompleto, ObjectDataNascimento dataNascimento, ObjectCPF cpf, ObjectEmail email, ObjectTelefone telefone) : base()
    {
        NomeCompleto = nomeCompleto;
        DataNascimento = dataNascimento;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
    }

    public Estudante()
    { }

    public ObjectNomeCompleto NomeCompleto { get; private set; }

    public ObjectDataNascimento DataNascimento { get; private set; }

    public ObjectCPF Cpf { get; private set; }

    public ObjectEmail Email { get; private set; }

    public ObjectTelefone Telefone { get; private set; }

    public ICollection<Matricula> Matriculas { get; private set; } = new List<Matricula>();

    public void AtualizarDados(ObjectNomeCompleto nome, ObjectEmail email, ObjectDataNascimento data, ObjectTelefone telefone)
    {
        this.NomeCompleto = nome;
        this.Email = email;
        this.DataNascimento = data;
        this.Telefone = telefone;
    }
}