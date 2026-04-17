using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicacao.Dtos.Matricola;

public record MatriculaDtoUpdate(

    [Required(ErrorMessage = "O status da matrícula é obrigatório.")]
    bool Ativo
);