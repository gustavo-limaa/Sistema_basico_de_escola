using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Intrinsics.X86;

namespace SistemaDeMatricula.Domain.Modelos;

public sealed class Professor : ModeloMain
{
    public Professor(ObjectNomeCompleto nomeCompleto,
        ObjectCPF cpf,
        ObjectEmail email,
        ValorMonetario salario,
        CategoriaProfessor categoria,
        ObjectDataNascimento dataNascimento,
        ObjectTelefone telefone) : base()
    {
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

    public String UsuarioId { get; private set; }
    public ObjectNomeCompleto NomeCompleto { get; private set; }

    public ObjectDataNascimento DataNascimento { get; private set; }

    public ObjectCPF Cpf { get; private set; }

    public ObjectEmail Email { get; private set; }

    public ObjectTelefone Telefone { get; private set; }

    public ValorMonetario Salario { get; private set; }

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
        if (!Ativo) throw new DomainException("Não é possível atualizar um professor desativado.");

        NomeCompleto = novoNome;
        Email = novoEmail;
        Salario = novoSalario;
        Categoria = novaCategoria;
        DataNascimento = novaDataNasc;
        Telefone = novoTelefone;
    }

    public void VincularUsuario(string usuarioIdDoToken)
    {
        UsuarioId = usuarioIdDoToken;
    }
}