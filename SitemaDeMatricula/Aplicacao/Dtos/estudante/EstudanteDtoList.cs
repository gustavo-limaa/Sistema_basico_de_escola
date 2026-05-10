namespace SistemaDeMatricula.Aplicacao.Dtos.estudante;

public sealed record EstudanteDtoList(
   Guid EstudanteId,
   string NomeCompleto,
   string Email);