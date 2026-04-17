using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicacao.Dtos.Matricola
{
    public record MatriculaDtoCreate(
        [Required(ErrorMessage = "O ID do estudante é obrigatório.")]
    Guid EstudanteId,
        [Required(ErrorMessage = "O ID da turma é obrigatório.")]
    Guid TurmaId
);
}