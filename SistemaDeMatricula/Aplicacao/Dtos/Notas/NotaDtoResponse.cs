using SistemaDeMatricula.Domain.Uteis;

namespace SistemaDeMatricula.Aplicacao.Dtos.Notas;

public record NotaDtoResponse(
    Guid Id,
    Guid MatriculaId,
    double Valor,
    string Descricao,
    TipoImportancia Importancia,
    CategoriaAvaliacao Categoria,
    DateTime DataEmissao
);