using System.ComponentModel.DataAnnotations;

namespace SistemaDeMatricula.Aplicacao.Dtos.Matricola;

public sealed record MatriculaDtoResponse
(

    Guid MatriculaId,

    DateTime DataMatricula,

    Guid EstudanteId,

    Guid TurmaId,

    bool Ativo
);