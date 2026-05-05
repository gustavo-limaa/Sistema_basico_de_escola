using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas
{
    public class ObterPorIdTurma
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

            // 1. Buscamos ignorando o filtro global para saber se ela existe "nas sombras"
            var turma = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(id);

            // 2. Se for null aqui, é porque o ID não existe mesmo (404 Real)
            if (turma is null)
                return Result<TurmaDtoResponse>.Falha("Turma não encontrada no sistema.");

            // 3. Se achou, mas está inativa, damos a mensagem específica que você sugeriu
            if (!turma.Ativo)
                return Result<TurmaDtoResponse>.Falha("Esta turma está desativada e não pode receber novas matrículas.");

            // 4. Se chegou aqui, ela está ativa e linda. Só converter e retornar.
            return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
        }
    }
}