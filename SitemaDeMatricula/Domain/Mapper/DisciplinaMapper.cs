using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Domain.Mapper;

public static class DisciplinaMapper
{
    // 1. Criar: Passamos apenas o que o construtor pede
    public static Disciplina ToDisciplina(this DisciplinaDtoCreate dto)
        => new Disciplina(dto.Nome, dto.CargaHoraria);

    // 2. Responder: O record aceita os campos diretos
    public static DisciplinaDtoResponse ToResponse(this Disciplina disciplina)
        => new DisciplinaDtoResponse(
            disciplina.DisciplinaId,
            disciplina.Nome.Valor,
            disciplina.CargaHoraria,
            disciplina.Ativo

        );

    // 3. Atualizar: Segue o método da Entidade
    public static void ToAtualizarDisciplina(this Disciplina disciplina, DisciplinaDtoUpdate dto)
    {
        // Atualiza os dados básicos
        disciplina.AtualizarDados(dto.Nome, dto.CargaHoraria);

        // Se o status no DTO for diferente do atual, a gente muda
        if (dto.Ativo) disciplina.Ativar(); else disciplina.Desativar();
    }
}