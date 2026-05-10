using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;

public static class TurmaMapper
{
    public static TurmaDtoResponse ToTurmaDtoResponse(this Turma turma)
    {
        return new TurmaDtoResponse(
            turma.TurmaId,
             turma.CodigoTurma.ValorFormatado,
            turma.CodigoTurma.Sigla,
            turma.CodigoTurma.Semestre,
            turma.CodigoTurma.Ano,
            turma.CodigoTurma.Numero,
            turma.Disciplina?.Nome ?? "Disciplina não carregada",
            turma.Professor?.NomeCompleto?.Valor ?? "Professor não carregado",
            turma.Ativo
        );
    }

    public static Turma ToTurma(this TurmaDtoCreate dto, CodigoTurma codigoValidado)
    {
        return new Turma(
            codigoValidado,
            dto.ProfessorId,
            dto.DisciplinaId
        );
    }

    public static void ToUpdateTurma(this Turma turma, TurmaDtoUpdate dto, CodigoTurma novoCodigo)
    {
        turma.AtualizarDados(
        novoCodigo,
        dto.ProfessorId,
        dto.DisciplinaId
    );
    }
}