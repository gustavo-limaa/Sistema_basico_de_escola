namespace SistemaDeMatricula.Aplicacao.Dtos.Disciplina;

public sealed record DisciplinaDtoResponse(
    Guid DisciplinaId,
    string Nome,
    int CargaHoraria,
    bool Ativo

);