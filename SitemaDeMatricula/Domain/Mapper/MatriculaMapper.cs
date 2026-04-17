using SitemaDeMatricula.Aplicacao.Dtos.Matricola;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Mapper
{
    public static class MatriculaMapper
    {
        public static MatriculaDtoResponse ToMatriculaDtoResponse(this Matricula matricula)
        {
            if (matricula == null) return null;
            return new MatriculaDtoResponse(
                MatriculaId: matricula.MatriculaId,
                DataMatricula: matricula.DataMatricula,
                EstudanteId: matricula.EstudanteId,
                TurmaId: matricula.TurmaId,
                Ativo: matricula.Ativo
            );
        }

        public static Matricula ToMatricula(this MatriculaDtoCreate dto)
        {
            if (dto == null) return null;
            return new Matricula
            (
              dto.EstudanteId, dto.TurmaId
            );
        }

        public static List<MatriculaDtoResponse> ToMatriculaDtoResponseList(this IEnumerable<Matricula> matriculas)
        {
            return matriculas.Select(m => m.ToMatriculaDtoResponse()).ToList();
        }

        public static MatriculaDtoUpdate ToMatriculaDtoUpdate(this Matricula matricula)
        {
            if (matricula == null) return null;
            return new MatriculaDtoUpdate(

                Ativo: matricula.Ativo
            );
        }
    }
}