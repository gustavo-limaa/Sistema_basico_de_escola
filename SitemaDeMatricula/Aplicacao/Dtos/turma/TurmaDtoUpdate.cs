using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicacao.Dtos.turma;

public record TurmaDtoUpdate(
    [Required(ErrorMessage = "Código da turma é obrigatório.")]
    string CodigoTurma,
    [Required(ErrorMessage = "ID do professor é obrigatório.")]
    Guid ProfessorId
);