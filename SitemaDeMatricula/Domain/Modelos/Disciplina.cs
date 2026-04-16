using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SitemaDeMatricula.Domain.Modelos;

public class Disciplina
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid DisciplinaId { get; private set; } = Guid.NewGuid();

    [Required(ErrorMessage = "O nome da disciplina é obrigatório.")]
    [MinLength(3, ErrorMessage = "O nome da disciplina deve ter pelo menos 3 caracteres.")]
    [MaxLength(100, ErrorMessage = "O nome da disciplina deve ter no máximo 100 caracteres.")]
    public NomeDisciplina Nome { get; private set; }

    [Required(ErrorMessage = "A carga horária é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A carga horária deve ser um valor positivo.")]
    public int CargaHoraria { get; private set; }

    [Required(ErrorMessage = "O status da disciplina é obrigatório.")]
    public bool Ativo { get; private set; } = true;

    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    public ICollection<Turma> Turmas { get; private set; } = new List<Turma>();

    // Construtor para o EF (sempre protegido/privado)
    protected Disciplina()
    { }

    public Disciplina(string nome, int cargaHoraria)
    {
        ValidarDados(nome, cargaHoraria);
        Nome = nome;
        CargaHoraria = cargaHoraria;
    }

    public void AtualizarDados(string nome, int cargaHoraria)
    {
        ValidarDados(nome, cargaHoraria);
        Nome = nome;
        CargaHoraria = cargaHoraria;
    }

    // Centralizando a validação para não repetir código
    private void ValidarDados(string nome, int cargaHoraria)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da disciplina é obrigatório.");

        if (cargaHoraria <= 0)
            throw new ArgumentException("A carga horária deve ser positiva.");
    }
}