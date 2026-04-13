using SitemaDeMatricula.Domain.Uteis;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SitemaDeMatricula.Domain.Modelos;

public class Professor
{
    // Construtor para garantir que o Professor nasça com dados válidos
    public Professor(ObjectNomeCompleto nome, ObjectCPF cpf, ObjectEmail email, ValorMonetario salario, CategoriaProfessor categoria)
    {
        ProfessorId = Guid.NewGuid();
        Nome = nome;
        Cpf = cpf;
        Email = email;
        Salario = salario;
        Categoria = categoria;
    }

    protected Professor()
    { } // EF Core

    [Key]
    public Guid ProfessorId { get; private set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public ObjectNomeCompleto Nome { get; private set; }

    [Required]
    public ObjectCPF Cpf { get; private set; }

    [Required]
    public ObjectEmail Email { get; private set; }

    public ObjectTelefone Telefone { get; private set; }

    [Required]
    public ValorMonetario Salario { get; private set; }

    [Required(ErrorMessage = "A categoria/disciplina é obrigatória.")]
    public CategoriaProfessor Categoria { get; private set; }
}