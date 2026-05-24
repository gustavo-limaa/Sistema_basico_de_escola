using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Domain.Mapper
{
    public static class NotasMapper
    {
        public static NotaDtoResponse ToNotaDtoResponse(this Nota nota)
        {
            if (nota == null) return null;
            return new NotaDtoResponse(
                Id: nota.Id,
                MatriculaId: nota.MatriculaId,
                Valor: nota.Valor,
                Descricao: nota.Descricao,
                Importancia: nota.Importancia,
                Categoria: nota.Categoria,
                DataEmissao: nota.DataEmissao
            );
        }

        public static Nota ToNota(this NotaDtoCreate notaDtoCreate, Guid matriculaId)
        {
            if (notaDtoCreate == null) return null;
            return new Nota(
                matriculaId: matriculaId,
                importancia: notaDtoCreate.Importancia,
                categoria: notaDtoCreate.Categoria,
                valor: notaDtoCreate.Valor,
                descricao: notaDtoCreate.Descricao,
                dataEmissao: DateTime.UtcNow
            );
        }

        public static void UpdateNota(this Nota nota, NotaDtoUpdate notaDtoUpdate)
        {
            if (nota == null || notaDtoUpdate == null) return;
            nota.AtualizarDados(
                valor: notaDtoUpdate.Valor,
                descricao: notaDtoUpdate.Descricao ?? nota.Descricao,
                importancia: notaDtoUpdate.Importancia ?? nota.Importancia,
                categoria: notaDtoUpdate.Categoria ?? nota.Categoria
            );
        }
    }
}