using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicacao.Dtos.Matricola;

public record MatriculaDtoResponse
(
    [Required(ErrorMessage = "O ID da matrícula é obrigatório.")]
    Guid MatriculaId,
    [Required(ErrorMessage = "A data da matrícula é obrigatória.")]
    DateTime DataMatricula,
    [Required(ErrorMessage = "O ID do estudante é obrigatório.")]
    Guid EstudanteId,
    [Required(ErrorMessage = "O ID da turma é obrigatório.")]
    Guid TurmaId,
    [Required(ErrorMessage = "O status da matrícula é obrigatório.")]
    bool Ativo
);