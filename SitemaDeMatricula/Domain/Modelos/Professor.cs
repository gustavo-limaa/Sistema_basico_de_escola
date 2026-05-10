using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Intrinsics.X86;

namespace SistemaDeMatricula.Domain.Modelos;

public sealed class Professor : ModeloMain
{
    public Professor(ObjectNomeCompleto nomeCompleto, ObjectCPF cpf, ObjectEmail email, ValorMonetario salario, CategoriaProfessor categoria, ObjectDataNascimento dataNascimento, ObjectTelefone telefone)
    {
        Id = Guid.NewGuid();
        Ativo = true;
        NomeCompleto = nomeCompleto;
        Cpf = cpf;
        Email = email;
        Salario = salario;
        Categoria = categoria;
        DataNascimento = dataNascimento;
        Telefone = telefone;
    }

    protected Professor()
    { } // EF Core

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

    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    public void AtualizarDados(

        ObjectNomeCompleto novoNome,
        ObjectEmail novoEmail,
        ValorMonetario novoSalario,
        CategoriaProfessor novaCategoria,
        ObjectDataNascimento novaDataNasc,
        ObjectTelefone novoTelefone)

    {
        if (!Ativo) throw new ArgumentException("Não é possível atualizar um professor desativado.");

        NomeCompleto = novoNome;
        Email = novoEmail;
        Salario = novoSalario;
        Categoria = novaCategoria;
        DataNascimento = novaDataNasc;
        Telefone = novoTelefone;
    }
}