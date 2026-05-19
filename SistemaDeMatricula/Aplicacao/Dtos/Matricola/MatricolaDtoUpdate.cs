using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.Matricola;

public sealed record MatriculaDtoUpdate(

    [Required(ErrorMessage = "O status da matrícula é obrigatório.")]
    bool Ativo
);