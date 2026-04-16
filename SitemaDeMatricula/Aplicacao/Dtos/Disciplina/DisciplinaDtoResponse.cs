using SitemaDeMatricula.Aplicaçao.Dtos.turma;

namespace SitemaDeMatricula.Aplicacao.Dtos.Disciplina;

public record DisciplinaDtoResponse(
    Guid DisciplinaId,
    string Nome,
    int CargaHoraria,
    bool Ativo

);