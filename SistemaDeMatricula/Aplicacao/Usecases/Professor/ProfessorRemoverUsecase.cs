using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

public sealed class ProfessorRemoverUsecase
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorRemoverUsecase(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid professorId)
    {
        if (professorId == Guid.Empty)
            return Result<bool>.Falha(MensagensProfessor.ProfessorInvalido);

        var professorExistente = await _repositorioProfessor.ObterPorIdAsync(professorId);

        if (professorExistente == null)
            return Result<bool>.Falha(MensagensProfessor.ProfessorNaoEncontrado);
        if (!professorExistente.Ativo)
            return Result<bool>.Falha(MensagensProfessor.ErroInativo_ou_Ativo);

        professorExistente.Desativar();

        _repositorioProfessor.Atualizar(professorExistente);

        var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

        return sucesso
            ? Result<bool>.Ok(true)
            : Result<bool>.Falha(MensagensProfessor.ErroSemAutoridade
            );
    }
}