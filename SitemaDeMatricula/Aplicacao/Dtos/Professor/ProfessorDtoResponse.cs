using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.Professor;

public sealed record ProfessorDtoResponse
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
    decimal Salario,
    [Required]
    string Categoria
 );