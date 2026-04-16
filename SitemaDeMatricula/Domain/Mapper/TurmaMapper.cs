using SitemaDeMatricula.Aplicaçao.Dtos.turma;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Mapper
{
    public static class TurmaMapper
    {
        public static TurmaDtoResponse ToTurmaDtoResponse(this Turma turma)
        {
            return new TurmaDtoResponse(
                turma.TurmaId,
                turma.CodigoTurma,
                turma.Disciplina.Nome,
                turma.Professor.NomeCompleto.Valor,
                turma.Ativo
            );
        }

        public static Turma ToTurma(this TurmaDtoUpdate turmaDto, Guid disciplinaId)
        {
            return new Turma(
                turmaDto.CodigoTurma,
                turmaDto.ProfessorId,
                disciplinaId
            );
        }

        public static void ToUpdateTurma(this Turma turma, TurmaDtoUpdate turmaDto)
        {
            turma.AtualizarDados(turmaDto.CodigoTurma, turmaDto.ProfessorId);
        }
    }
}