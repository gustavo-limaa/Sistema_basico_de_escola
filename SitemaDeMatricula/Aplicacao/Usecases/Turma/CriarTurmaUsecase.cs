using SitemaDeMatricula.Aplicaçao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicaçao.Usecases.Turmas;

public class CriarTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;
    private readonly IRepositorioProfessor _profRepo;
    private readonly IDisciplinaRepositorio _discRepo;

    public CriarTurmaUseCase(
        IRepositorioTurma turmaRepo,
        IRepositorioProfessor profRepo,
        IDisciplinaRepositorio discRepo)
    {
        _turmaRepo = turmaRepo;
        _profRepo = profRepo;
        _discRepo = discRepo;
    }

    public async Task<Result<Guid>> ExecutarAsync(TurmaDtoCreate dto)
    {
        // 1. Validar se o Professor existe
        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<Guid>.Falha("Professor não encontrado.");

        // 2. DESAFIO: Validar se a Disciplina existe
        // Dica: Use o _discRepo e o dto.DisciplinaId
        // [ESCREVA SEU CÓDIGO AQUI]

        // 3. Criar a entidade (O construtor que definimos)
        var novaTurma = new Turma(dto.CodigoTurma, dto.ProfessorId, dto.DisciplinaId);

        // 4. Salvar (O seu repositório já chama o SaveChanges!)
        await _turmaRepo.AdicionarAsync(novaTurma);

        return Result<Guid>.Ok(novaTurma.TurmaId);
    }
}