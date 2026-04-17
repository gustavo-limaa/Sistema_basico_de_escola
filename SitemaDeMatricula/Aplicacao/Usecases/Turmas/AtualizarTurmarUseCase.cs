using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper; // Certifique-se de importar seu Mapper

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

public class AtualizarTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;
    private readonly IRepositorioProfessor _profRepo;

    public AtualizarTurmaUseCase(IRepositorioTurma turmaRepo, IRepositorioProfessor profRepo)
    {
        _turmaRepo = turmaRepo;
        _profRepo = profRepo;
    }

    public async Task<Result<TurmaDtoResponse>> ExecutarAsync(Guid turmaId, TurmaDtoUpdate dto)
    {
        // 1. Validar se a Turma existe (com Include para o Mapper funcionar depois)
        var turmaExistente = await _turmaRepo.ObterPorIdAsync(turmaId);
        if (turmaExistente == null)
            return Result<TurmaDtoResponse>.Falha("Turma não encontrada.");

        // 2. Validar se o novo Professor existe
        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<TurmaDtoResponse>.Falha("Professor informado não encontrado.");

        // 3. Atualizar os dados usando o comportamento do Domínio
        turmaExistente.AtualizarDados(dto.CodigoTurma, dto.ProfessorId);

        // 4. Persistir no banco
        var sucesso = await _turmaRepo.AtualizarAsync(turmaExistente);

        if (!sucesso)
            return Result<TurmaDtoResponse>.Falha("Erro ao persistir as alterações da turma.");

        // 5. Retornar o DTO atualizado usando seu Mapper
        return Result<TurmaDtoResponse>.Ok(turmaExistente.ToTurmaDtoResponse());
    }
}