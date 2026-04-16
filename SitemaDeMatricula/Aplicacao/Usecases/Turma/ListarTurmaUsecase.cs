using SitemaDeMatricula.Aplicaçao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turma
{
    public class ListarTurmaUsecase
    {
        private readonly IRepositorioTurma _turmaRepo;

        public ListarTurmaUsecase(IRepositorioTurma turmaRepo)
        {
            _turmaRepo = turmaRepo;
        }

        public async Task<Result<IEnumerable<TurmaDtoResponse>>> ExecutarAsync()
        {
            var turmas = await _turmaRepo.ListarTodasAsync();

            // Se turmas for uma lista vazia, o Select apenas retorna outra lista vazia.
            var turmasDto = turmas.Select(t => t.ToTurmaDtoResponse()).ToList();

            return Result<IEnumerable<TurmaDtoResponse>>.Ok(turmasDto);
        }
    }
}