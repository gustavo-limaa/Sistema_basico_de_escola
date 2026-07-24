using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas;

public sealed class ObterPorCodigoTurma
{
    private readonly IRepositorioTurma _turmaRepo;

    public ObterPorCodigoTurma(IRepositorioTurma turmaRepo)
    {
        _turmaRepo = turmaRepo;
    }

    public async Task<Result<TurmaDtoResponse>> ExecutarAsync(string codigoTurma)
    {
        if (string.IsNullOrWhiteSpace(codigoTurma))
            return Result<TurmaDtoResponse>.Falha(MensagensTurma.CodigoTurmaObrigatorio);

        var turma = await _turmaRepo.ObterPorCodigoAsync(codigoTurma);
        if (turma == null)
            return Result<TurmaDtoResponse>.Falha(MensagensTurma.TurmaNaoEncontrada);

        return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
    }
}