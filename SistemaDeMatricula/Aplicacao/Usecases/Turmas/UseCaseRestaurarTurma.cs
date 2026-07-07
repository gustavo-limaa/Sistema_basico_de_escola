using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas;

public sealed class RestaurarTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;

    public RestaurarTurmaUseCase(IRepositorioTurma turmaRepo)
    {
        _turmaRepo = turmaRepo;
    }

    public async Task<Result<TurmaDtoResponse>> ExecutarAsync(Guid id)
    {
        // 1. Obtém a entidade (sem travar o estado no EF, ou tratando-a)
        var turma = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(id);

        if (turma == null)
            return Result<TurmaDtoResponse>.Falha("Turma não encontrada.");

        // 🎯 CASO 1: Se a turma JÁ ESTIVER ATIVA, não faz nada e retorna sucesso
        // Isso vai fazer o seu primeiro teste unitário passar voando (Times.Never)!
        if (turma.Ativo)
        {
            return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
        }

        // 🎯 CASO 2: Se a turma estava inativa, agora sim nós mudamos o estado dela
        turma.Ativar(); // Altera a propriedade na memória para o teste unitário passar!

        // Chama o método do repositório responsável por persistir essa mudança no banco real
        var persistiu = await _turmaRepo.RestaurarAsync(id);

        if (!persistiu)
            return Result<TurmaDtoResponse>.Falha("Não foi possível salvar as alterações no banco.");

        return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
    }
}// mapeamento de erros para código HTTP