using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.turma;

public sealed record TurmaDtoCreate(
    [Required(ErrorMessage = "ID da disciplina é obrigatório.")]
    Guid DisciplinaId,
    [Required(ErrorMessage = "ID do professor é obrigatório.")]
    Guid ProfessorId,
    int CapacidadeMaxima,
    [Required] string Sigla,
    [Range(1, 2)] int Semestre,
    [Required] int AnoLetivo,
    [Required] int Numero);