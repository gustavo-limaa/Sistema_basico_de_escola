using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
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
        var turma = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(id);

        if (turma == null)
            return Result<TurmaDtoResponse>.Falha(MensagensTurma.TurmaNaoEncontrada);

        if (turma.Ativo)
        {
            return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
        }

        turma.Ativar();

        var persistiu = await _turmaRepo.RestaurarAsync(id);

        if (!persistiu)
            return Result<TurmaDtoResponse>.Falha(MensagensTurma.ErroPersistenciaBanco);

        return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
    }
}