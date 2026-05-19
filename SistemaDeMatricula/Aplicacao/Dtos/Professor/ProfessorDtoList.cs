using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.Professor;

public sealed record ProfessorDtoList(
    [Required]
    Guid ProfessorId,
    [Required][MinLength(3)][MaxLength(80)]
    string NomeCompleto,
    [Required][EmailAddress]
    string Email,
    [Required]
    DateOnly DataNascimento,
    [Required]
    string Categoria
);