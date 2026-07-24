using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Value_Object;

namespace SistemaDeMatricula.Domain.Mapper;

public static class DisciplinaMapper
{
    public static Disciplina ToDisciplina(this DisciplinaDtoCreate dto)
        => new Disciplina(dto.Nome, new CargaHoraria(dto.CargaHoraria));

    public static DisciplinaDtoResponse ToResponse(this Disciplina disciplina)
        => new DisciplinaDtoResponse(
            disciplina.Id,
            disciplina.Nome.Valor,
            disciplina.CargaHoraria.Valor,
            disciplina.Ativo

        );

    public static void ToAtualizarDisciplina(this Disciplina disciplina, DisciplinaDtoUpdate dto)
    {
        disciplina.AtualizarDados(dto.Nome, dto.CargaHoraria);

        if (dto.Ativo) disciplina.Ativar(); else disciplina.Desativar();
    }
}