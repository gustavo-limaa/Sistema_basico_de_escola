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

        // Mudança para 404 - Evita erro de semântica
        if (turma == null) return Result<object?>.NaoEncontrado("Turma não encontrada.");

        // 1. Idempotência - Já está verde nos seus testes
        if (!turma.Ativo) return Result<object?>.Ok(null);

        // 2. Trava de segurança contra alunos ativos
        var temAlunosAtivos = await _turmaMatriculaRepo.ExisteQualquerMatriculaAtivaParaTurmaAsync(id);
        if (temAlunosAtivos)
        {
            // Certifique-se que o TipoErro aqui seja Validacao/Falha para gerar o 400
            return Result<object?>.Falha("Não é possível desativar uma turma com alunos matriculados.");
        }

        turma.Desativar();
        await _turmaRepo.AtualizarAsync(turma);

        return Result<object?>.Ok(null); // Retorna Sucesso (204 No Content no Controller)
    }
}