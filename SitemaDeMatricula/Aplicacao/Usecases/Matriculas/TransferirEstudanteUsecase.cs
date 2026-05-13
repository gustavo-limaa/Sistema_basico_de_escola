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
            // 1. Busca a matrícula antiga (Tracking ligado para o Desativar)
            var matriculaAntiga = await _uow.Matriculas.ObterPorIdAsync(matriculaId);
            if (matriculaAntiga == null)
                return Result<MatriculaDtoResponse>.Falha("Matrícula original não encontrada.");

            // 2. Busca a nova turma
            var novaTurma = await _uow.Turmas.ObterPorIdAsync(novaTurmaId);
            if (novaTurma == null)
                return Result<MatriculaDtoResponse>.Falha("A nova turma não existe.");

            // 💡 REGRA DE OURO: Validar se a nova turma tem vaga antes de mexer em qualquer coisa
            // Supondo que você adicione esse método no seu repositório ou entidade:
            var vagasOcupadas = await _uow.Matriculas.ContarMatriculasAtivasNaTurmaAsync(novaTurmaId);
            if (vagasOcupadas >= novaTurma.CapacidadeMaxima) // Exemplo de propriedade na Turma
                return Result<MatriculaDtoResponse>.Falha("A nova turma já atingiu o limite de alunos.");

            // 3. Orquestração das mudanças (Em memória)
            matriculaAntiga.Desativar();
            await _uow.Matriculas.AtualizarAsync(matriculaAntiga);

            var novaMatricula = new Matricula(matriculaAntiga.EstudanteId, novaTurmaId);
            await _uow.Matriculas.AdicionarAsync(novaMatricula);

            // 4. Persistência Atômica via Maestro
            var sucesso = await _uow.CommitAsync();

            if (!sucesso)
                return Result<MatriculaDtoResponse>.Falha("Falha técnica ao processar a transferência no banco de dados.");

            // 5. Retorno com Mapper
            return Result<MatriculaDtoResponse>.Ok(novaMatricula.ToMatriculaDtoResponse());
        }
    }
}