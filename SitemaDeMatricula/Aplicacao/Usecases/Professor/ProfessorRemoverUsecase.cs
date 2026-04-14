using SitemaDeMatricula.Aplicaçao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorRemoverUsecase
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorRemoverUsecase(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(Guid professorId)
    {
        // 1. Fail Fast
        if (professorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha("ID do professor é inválido.");
        // 2. Busca o professor existente
        var professorExistente = await _repositorioProfessor.ObterPorIdAsync(professorId);

        if (professorExistente == null)
            return Result<ProfessorDtoResponse>.Falha("Professor não encontrado.");
        // 3. Remove o professor

        _repositorioProfessor.Remover(professorExistente);
        // 4. Salva as alterações
        var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();
        // 5. Retorna o resultado
        return sucesso ? Result<ProfessorDtoResponse>.Ok(professorExistente.ToProfessorDtoResponse()) : Result<ProfessorDtoResponse>.Falha("Erro ao remover o professor.");
    }
}