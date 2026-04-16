using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicacao.Dtos.Professor;

public record ProfessorDtoResponse
 (
    [Required]
    Guid ProfessorId,
    [Required][MaxLength(80)][MinLength(3)]
    string NomeCompleto,
    [Required]
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