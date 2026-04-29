using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorAtualizarUsecase
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorAtualizarUsecase(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(ProfessorDtoUpdate professorDto)
    {
        // 1. Fail Fast
        if (professorDto == null || professorDto.ProfessorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha("Dados do professor são inválidos.");
        // 2. Busca o professor existente
        var professorExistente = await _repositorioProfessor.ObterPorIdAsync(professorDto.ProfessorId);
        if (professorExistente == null)
            return Result<ProfessorDtoResponse>.Falha("Professor não encontrado.");

        // Checagem de E-mail (Para bater com o seu teste!)
        var professorComMesmoEmail = await _repositorioProfessor.ObterPorEmailAsync(professorDto.Email);

        // Só falha se achou alguém E esse alguém NÃO for o professor que estamos editando
        if (professorComMesmoEmail != null && professorComMesmoEmail.ProfessorId != professorDto.ProfessorId)
        {
            return Result<ProfessorDtoResponse>.Falha("Já existe outro professor cadastrado com este e-mail.");
        }
        professorExistente.ToAtualizarProfessor(professorDto);

        // 4. Salva as alterações
        _repositorioProfessor.Atualizar(professorExistente);
        var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();
        // 5. Retorna o resultado
        return sucesso ? Result<ProfessorDtoResponse>.Ok(professorExistente.ToProfessorDtoResponse()) : Result<ProfessorDtoResponse>.Falha("Erro ao atualizar o professor.");
    }
}