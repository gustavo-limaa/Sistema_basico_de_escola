using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

public class RemoverTurmaUseCase
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
        if (turma == null) return Result<object?>.Falha("Turma não encontrada.");

        // 1. PRIORIDADE: Se já está inativa, sucesso imediato (Idempotência)
        if (!turma.Ativo) return Result<object?>.Ok(null);

        // 2. Só agora verificamos a trava de segurança
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