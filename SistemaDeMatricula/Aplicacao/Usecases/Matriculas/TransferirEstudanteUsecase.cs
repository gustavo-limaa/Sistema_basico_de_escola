using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas
{
    public sealed class TransferirEstudanteUsecase
    {
        private readonly IUnitOfWork _uow;

        public TransferirEstudanteUsecase(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<MatriculaDtoResponse>> ExecutarAsync(Guid matriculaId, Guid novaTurmaId)
        {
            var matriculaAntiga = await _uow.Matriculas.ObterPorIdAsync(matriculaId);
            if (matriculaAntiga == null)
                return Result<MatriculaDtoResponse>.NaoEncontrado("Matrícula original não encontrada.");

            var novaTurma = await _uow.Turmas.ObterPorIdAsync(novaTurmaId);
            if (novaTurma == null)
                return Result<MatriculaDtoResponse>.Falha("A nova turma não existe.");
            var vagasOcupadas = await _uow.Matriculas.ContarMatriculasAtivasNaTurmaAsync(novaTurmaId);
            if (vagasOcupadas >= novaTurma.CapacidadeMaxima)
                return Result<MatriculaDtoResponse>.Falha("A nova turma já atingiu o limite de alunos.");

            matriculaAntiga.Desativar();
            await _uow.Matriculas.AtualizarAsync(matriculaAntiga);

            var novaMatricula = new Matricula(matriculaAntiga.EstudanteId, novaTurmaId);
            await _uow.Matriculas.AdicionarAsync(novaMatricula);

            var sucesso = await _uow.CommitAsync();

            if (!sucesso)
                return Result<MatriculaDtoResponse>.Falha("Falha técnica ao processar a transferência no banco de dados.");

            return Result<MatriculaDtoResponse>.Ok(novaMatricula.ToMatriculaDtoResponse());
        }
    }
}