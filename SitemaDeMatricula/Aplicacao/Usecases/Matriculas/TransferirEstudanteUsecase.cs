using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas
{
    public sealed class TransferirEstudanteUsecase
    {
        private readonly IRepositorioMatricula _matriculaRepo;
        private readonly IRepositorioTurma _turmaRepo;

        public TransferirEstudanteUsecase(IRepositorioMatricula matriculaRepo, IRepositorioTurma turmaRepo)
        {
            _matriculaRepo = matriculaRepo;
            _turmaRepo = turmaRepo;
        }

        public async Task<Result<MatriculaDtoResponse>> ExecutarAsync(Guid matriculaId, Guid novaTurmaId)
        {
            var matriculaAntiga = await _matriculaRepo.ObterPorIdAsync(matriculaId);
            if (matriculaAntiga == null) return Result<MatriculaDtoResponse>.Falha("Matrícula original não encontrada.");

            var novaTurma = await _turmaRepo.ObterPorIdAsync(novaTurmaId);
            if (novaTurma == null) return Result<MatriculaDtoResponse>.Falha("A nova turma não existe.");

            matriculaAntiga.Desativar();
            await _matriculaRepo.AtualizarAsync(matriculaAntiga);

            var novaMatricula = new Matricula(matriculaAntiga.EstudanteId, novaTurmaId);
            await _matriculaRepo.AdicionarAsync(novaMatricula);

            return Result<MatriculaDtoResponse>.Ok(novaMatricula.ToMatriculaDtoResponse());
        }
    }
}