using SitemaDeMatricula.Aplicacao.Dtos.Professor;
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

    public async Task<Result<bool>> ExecutarAsync(Guid professorId)
    {
        // 1. Fail Fast (Sempre protegendo a entrada)
        if (professorId == Guid.Empty)
            return Result<bool>.Falha("ID do professor é inválido.");

        // 2. Busca o professor (Aqui o Global Filter garante que só pegamos quem está Ativo)
        var professorExistente = await _repositorioProfessor.ObterPorIdAsync(professorId);

        if (professorExistente == null)
            return Result<bool>.Falha("Professor não encontrado.");

        // 3. A MÁGICA ACONTECE AQUI:
        // Em vez de _repositorio.Remover(), chamamos o método de domínio:
        professorExistente.Desativar();

        // 4. Avisamos ao Repositório que houve uma MUDANÇA (Update) e não uma Exclusão
        _repositorioProfessor.Atualizar(professorExistente);

        // 5. Persistência
        var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

        // 6. Retorno (Ajustando a mensagem para refletir a realidade)
        return sucesso
            ? Result<bool>.Ok(true)
            : Result<bool>.Falha("Erro ao desativar o professor no banco de dados.");
    }
}