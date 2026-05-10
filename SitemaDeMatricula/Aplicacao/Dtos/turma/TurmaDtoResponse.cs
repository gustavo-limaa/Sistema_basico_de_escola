namespace SitemaDeMatricula.Aplicacao.Dtos.turma;

public record TurmaDtoResponse(
    Guid Id,
    string CodigoFormatado,
    string sigla,// Ex: "MAT-2026-1-001"
    int Semestre,
    int AnoLetivo,
    int Numero,
    string NomeDisciplina,
    string NomeProfessor,
    bool Ativo);