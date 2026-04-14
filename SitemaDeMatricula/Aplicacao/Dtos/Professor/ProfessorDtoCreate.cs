using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SitemaDeMatricula.Aplicaçao.Dtos.Professor;

public record ProfessorDtoCreate
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
    string Salario,
    [Required]
    string Categoria

);