namespace SitemaDeMatricula.Aplicacao.Dtos.estudante;

public record EstudanteDtoList(
   Guid EstudanteId,
   string NomeCompleto,
   string Email // Opcional, as vezes útil pMara identificar
);