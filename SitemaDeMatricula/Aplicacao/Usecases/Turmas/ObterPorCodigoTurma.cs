using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

public class ObterPorCodigoTurma
{
    private readonly IRepositorioTurma _turmaRepo;

    public ObterPorCodigoTurma(IRepositorioTurma turmaRepo)
    {
        _turmaRepo = turmaRepo;
    }

    public async Task<Result<TurmaDtoResponse>> ExecutarAsync(string codigoTurma)
    {
        if (string.IsNullOrWhiteSpace(codigoTurma))
            return Result<TurmaDtoResponse>.Falha("Código da turma é obrigatório.");

        var turma = await _turmaRepo.ObterPorCodigoAsync(codigoTurma);
        if (turma == null)
            return Result<TurmaDtoResponse>.Falha("Turma não encontrada.");

        return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
    }
}