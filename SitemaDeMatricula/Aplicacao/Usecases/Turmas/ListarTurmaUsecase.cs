using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

public class ListarTurmaUsecase
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
        // O Repositório já deve ter o .Include(t => t.Professor).Include(t => t.Disciplina)
        // E também o .AsNoTracking() para performance!
        var turmas = await _turmaRepo.ListarTodasAsync();

        // O Mapper resolve o problema de Professor/Disciplina inativos (que vêm nulos)
        var turmasDto = turmas.Select(t => t.ToTurmaDtoResponse()).ToList();

        return Result<IEnumerable<TurmaDtoResponse>>.Ok(turmasDto);
    }
}