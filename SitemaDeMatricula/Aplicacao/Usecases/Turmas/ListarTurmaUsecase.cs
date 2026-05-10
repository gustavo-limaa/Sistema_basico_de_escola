using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas;

public sealed class ListarTurmaUsecase
{
    private readonly IRepositorioTurma _turmaRepo;
    private readonly IRepositorioProfessor _professorRepo;
    private readonly IDisciplinaRepositorio _disciplinaRepo;

    public ListarTurmaUsecase(IRepositorioTurma turmaRepo, IRepositorioProfessor professorRepo, IDisciplinaRepositorio disciplinaRepo)
    {
        _turmaRepo = turmaRepo;
        _professorRepo = professorRepo;
        _disciplinaRepo = disciplinaRepo;
    }

    public async Task<Result<IEnumerable<TurmaDtoResponse>>> ExecutarAsync()
    {
        var turmas = await _turmaRepo.ListarTodasAsync();

        var turmasDto = turmas.Select(t => t.ToTurmaDtoResponse()).ToList();

        return Result<IEnumerable<TurmaDtoResponse>>.Ok(turmasDto);
    }
}