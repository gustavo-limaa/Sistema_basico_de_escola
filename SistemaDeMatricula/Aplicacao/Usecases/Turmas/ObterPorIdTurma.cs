using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas
{
    public sealed class ObterPorIdTurma
    {
        private readonly IRepositorioTurma _turmaRepo;

        public ObterPorIdTurma(IRepositorioTurma turmaRepo)
        {
            _turmaRepo = turmaRepo;
        }

        public async Task<Result<TurmaDtoResponse>> ExecutarAsync(Guid id)
        {
            if (id == Guid.Empty)
                return Result<TurmaDtoResponse>.Falha("ID inválido.");

            var turma = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(id);

            if (turma is null)
                return Result<TurmaDtoResponse>.Falha("Turma não encontrada no sistema.");

            if (!turma.Ativo)
                return Result<TurmaDtoResponse>.Falha("Esta turma está desativada e não pode receber novas matrículas.");

            return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
        }
    }
}