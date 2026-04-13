using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SitemaDeMatricula.Domain.Modelos;

public class Estudante
{
    public Estudante(Guid estudanteId, ObjectNomeCompleto nomeCompleto, ObjectDataNascimento dataNascimento, ObjectCPF cpf, ObjectEmail email, ObjectTelefone telefone)
    {
        EstudanteId = Guid.NewGuid();
        NomeCompleto = nomeCompleto;
        DataNascimento = dataNascimento;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
    }

    public Estudante()
    { }

    [Key]
    public Guid EstudanteId { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    public ObjectNomeCompleto NomeCompleto { get; set; }

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    [Column(TypeName = "date")]
    [DataType(DataType.Date)]
    public ObjectDataNascimento DataNascimento { get; set; }

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    public ObjectCPF Cpf { get; set; }

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
    [MaxLength(150)]
    public ObjectEmail Email { get; set; }

    [Required(ErrorMessage = "O telefone de contato é obrigatório.")]
    public ObjectTelefone Telefone { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    public void AtualizarDados(ObjectNomeCompleto nome, ObjectEmail email, ObjectDataNascimento data, ObjectTelefone telefone)
    {
        this.NomeCompleto = nome;
        this.Email = email;
        this.DataNascimento = data;
        this.Telefone = telefone;
    }
}