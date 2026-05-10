using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.Matricola;

public sealed record MatriculaDtoCreate(
    [Required(ErrorMessage = "O ID do estudante é obrigatório.")]
Guid EstudanteId,
    [Required(ErrorMessage = "O ID da turma é obrigatório.")]
Guid TurmaId
);