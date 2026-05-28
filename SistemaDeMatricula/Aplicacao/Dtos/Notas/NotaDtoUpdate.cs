using SistemaDeMatricula.Domain.Uteis;

namespace SistemaDeMatricula.Aplicacao.Dtos.Notas;

public sealed record NotaDtoUpdate(

   double Valor,
   string? Descricao,
   TipoImportancia? Importancia,
   CategoriaAvaliacao? Categoria
    );