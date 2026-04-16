using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicacao.Dtos.Disciplina;

public record DisciplinaDtoCreate(
   [Required( ErrorMessage = "O nome da disciplina é obrigatório.")][MinLength(3, ErrorMessage = "O nome da disciplina deve ter pelo menos 3 caracteres.") ][MaxLength(100,ErrorMessage = "O nome da disciplina deve ter entre 3 e 100 caracteres.")]
    string Nome,
   [Required( ErrorMessage = "A carga horária é obrigatória.")][Range(1, int.MaxValue, ErrorMessage = "A carga horária deve ser um valor positivo.")]
    int CargaHoraria

);