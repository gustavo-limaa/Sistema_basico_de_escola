using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.turma;

public sealed record TurmaDtoUpdate(

    [Required(ErrorMessage = "ID do professor é obrigatório.")]
    Guid ProfessorId,
    [Required(ErrorMessage = "ID da disciplina é obrigatório.")]
    Guid DisciplinaId,
    int novaCapacidade,
    [Required(ErrorMessage = "O status da turma é obrigatório.")]
    bool Ativo,
    [Required]
    string Sigla,
    [Range(1, 2)]
     int Semestre,
    [Required]
    int AnoLetivo,
    [Required]
    int Numero

);