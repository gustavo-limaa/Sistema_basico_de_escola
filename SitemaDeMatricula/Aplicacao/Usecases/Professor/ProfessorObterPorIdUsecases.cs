using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorObterPorIdUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorObterPorIdUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(Guid professorId)
    {
        // 1. Fail Fast (Sempre no topo)
        if (professorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha("ID do professor é inválido.");

        // 2. Busca (Ação principal)
        var professor = await _repositorioProfessor.ObterPorIdAsync(professorId);

        // 3. Validação do resultado
        if (professor == null)
            return Result<ProfessorDtoResponse>.Falha("Professor não encontrado.");

        // 4. Transformação e Retorno
        return Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse());
    }
}