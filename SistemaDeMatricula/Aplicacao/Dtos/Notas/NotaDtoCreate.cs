using SistemaDeMatricula.Domain.Uteis;

namespace SistemaDeMatricula.Aplicacao.Dtos.Notas;

public record NotaDtoCreate(

    double Valor,
    string Descricao,
    TipoImportancia Importancia,
    CategoriaAvaliacao Categoria

);