using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicaçao.Dtos.turma;

public record TurmaDtoCreate(
[Required(ErrorMessage = "Código da turma é obrigatório.")]
 string CodigoTurma,
[Required(ErrorMessage = "ID da disciplina é obrigatório.")]
 Guid DisciplinaId,
[Required(ErrorMessage = "ID do professor é obrigatório.")]
 Guid ProfessorId
);