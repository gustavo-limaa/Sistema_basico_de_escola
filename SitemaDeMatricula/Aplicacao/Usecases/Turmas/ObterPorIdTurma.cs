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

            var turma = await _turmaRepo.ObterPorIdAsync(id);
            if (turma == null)
                return Result<TurmaDtoResponse>.Falha("Turma não encontrada.");

            return Result<TurmaDtoResponse>.Ok(turma.ToTurmaDtoResponse());
        }
    }
}