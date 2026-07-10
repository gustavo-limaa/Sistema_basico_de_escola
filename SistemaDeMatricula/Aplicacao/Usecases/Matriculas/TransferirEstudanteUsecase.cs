using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
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
                return Result<MatriculaDtoResponse>.NaoEncontrado(MensagensMatricula.MatriculaNaoEncontrada);

            var novaTurma = await _uow.Turmas.ObterPorIdAsync(novaTurmaId);
            if (novaTurma == null)
                return Result<MatriculaDtoResponse>.Falha(MensagensTurma.TurmaNaoEncontrada);
            var vagasOcupadas = await _uow.Matriculas.ContarMatriculasAtivasNaTurmaAsync(novaTurmaId);
            if (vagasOcupadas >= novaTurma.CapacidadeMaxima)
                return Result<MatriculaDtoResponse>.Falha(MensagensTurma.TurmaLotada);

            matriculaAntiga.Desativar();
            await _uow.Matriculas.AtualizarAsync(matriculaAntiga);

            var novaMatricula = new Matricula(matriculaAntiga.EstudanteId, novaTurmaId);
            await _uow.Matriculas.AdicionarAsync(novaMatricula);

            var sucesso = await _uow.CommitAsync();

            if (!sucesso)
                return Result<MatriculaDtoResponse>.Falha(MensagensMatricula.ErroPersistenciaBanco);

            return Result<MatriculaDtoResponse>.Ok(novaMatricula.ToMatriculaDtoResponse());
        }
    }
}