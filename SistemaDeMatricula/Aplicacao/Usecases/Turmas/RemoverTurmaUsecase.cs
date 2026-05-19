using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas;

public sealed class RemoverTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;
    private readonly IRepositorioMatricula _turmaMatriculaRepo;
    private readonly IRepositorioEstudante _turmaEstudanteRepo;

    public RemoverTurmaUseCase(IRepositorioTurma turmaRepo, IRepositorioMatricula turmaMatriculaRepo, IRepositorioEstudante turmaEstudanteRepo)
    {
        _turmaRepo = turmaRepo;
        _turmaMatriculaRepo = turmaMatriculaRepo;
        _turmaEstudanteRepo = turmaEstudanteRepo;
    }

    public async Task<Result<object?>> ExecutarAsync(Guid id)
    {
        var turma = await _turmaRepo.ObterPorIdAsync(id);

        if (turma == null) return Result<object?>.NaoEncontrado("Turma não encontrada.");

        if (!turma.Ativo) return Result<object?>.Ok(null);

        var temAlunosAtivos = await _turmaMatriculaRepo.ExisteQualquerMatriculaAtivaParaTurmaAsync(id);
        if (temAlunosAtivos)
        {
            return Result<object?>.Falha("Não é possível desativar uma turma com alunos matriculados.");
        }

        turma.Desativar();
        await _turmaRepo.AtualizarAsync(turma);

        return Result<object?>.Ok(null);
    }
}