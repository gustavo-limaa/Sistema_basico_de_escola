using SitemaDeMatricula.Domain.Uteis;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Intrinsics.X86;

namespace SitemaDeMatricula.Domain.Modelos;

public class Professor
{
    // Construtor para garantir que o Professor nasça com dados válidos
    public Professor(ObjectNomeCompleto nomeCompleto, ObjectCPF cpf, ObjectEmail email, ValorMonetario salario, CategoriaProfessor categoria, ObjectDataNascimento dataNascimento)
    {
        ProfessorId = Guid.NewGuid();
        NomeCompleto = nomeCompleto;
        Cpf = cpf;
        Email = email;
        Salario = salario;
        Categoria = categoria;
        DataNascimento = dataNascimento;
    }

    protected Professor()
    { } // EF Core

    [Key]
    public Guid ProfessorId { get; private set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public ObjectNomeCompleto NomeCompleto { get; private set; }

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    public ObjectDataNascimento DataNascimento { get; private set; }

    [Required]
    public ObjectCPF Cpf { get; private set; }

    [Required]
    public ObjectEmail Email { get; private set; }

    public ObjectTelefone Telefone { get; private set; }

    [Required]
    public ValorMonetario Salario { get; private set; }

    [Required(ErrorMessage = "A categoria/disciplina é obrigatória.")]
    public CategoriaProfessor Categoria { get; private set; }

    public void AtualizarDados(
        ObjectNomeCompleto novoNome,
        ObjectEmail novoEmail,
        ValorMonetario novoSalario,
        CategoriaProfessor novaCategoria,
        ObjectDataNascimento novaDataNasc,
        ObjectTelefone novoTelefone)
    {
        // Aqui você pode adicionar lógica extra se precisar,
        // mas os próprios Value Objects já garantem a validação.

        NomeCompleto = novoNome;
        Email = novoEmail;
        Salario = novoSalario;
        Categoria = novaCategoria;
        DataNascimento = novaDataNasc;
        Telefone = novoTelefone;
    }
}