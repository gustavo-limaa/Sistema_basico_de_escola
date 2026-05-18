namespace SistemaDeMatricula.Aplicacao.Dtos.turma;

public sealed record TurmaDtoResponse(
    Guid Id,
    string CodigoFormatado,
    string sigla,// Ex: "MAT-2026-1-001"
    int Semestre,
    int AnoLetivo,
    int Numero,
    int capacidadeMaxima,
    string NomeDisciplina,
    string NomeProfessor,
    bool Ativo);