using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.Professor;

public sealed record ProfessorDtoCreate
(
    [Required][MaxLength(80)][MinLength(3)]
    string NomeCompleto,
    [Required] [RegularExpression(@"^\d{3}\d{3}\d{3}\d{2}$")]
    string Cpf,
    [Required]
    DateOnly DataNascimento,
    [Required][EmailAddress]
    string Email,
    [Required][Phone]
    string Telefone,
    [Required]
    Decimal Salario,
    [Required]
    string Categoria

);