using SitemaDeMatricula.Aplicacao.Dtos;

namespace SitemaDeMatricula.Aplicacao.Dtos.Disciplina;

public record DisciplinaDtoResponse(
    Guid DisciplinaId,
    string Nome,
    int CargaHoraria,
    bool Ativo

);