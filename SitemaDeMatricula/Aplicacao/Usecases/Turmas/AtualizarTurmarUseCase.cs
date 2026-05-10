using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas;

public sealed class AtualizarTurmaUseCase
{
    private readonly IRepositorioTurma _turmaRepo;
    private readonly IRepositorioProfessor _profRepo;
    private readonly IDisciplinaRepositorio _disciplinaRepo;

    public AtualizarTurmaUseCase(IRepositorioTurma turmaRepo, IRepositorioProfessor profRepo, IDisciplinaRepositorio disciplinaRepo)
    {
        _turmaRepo = turmaRepo;
        _profRepo = profRepo;
        _disciplinaRepo = disciplinaRepo;
    }

    public async Task<Result<TurmaDtoResponse>> ExecutarAsync(Guid turmaId, TurmaDtoUpdate dto)
    {
        var turmaParaEditar = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(turmaId);

        if (turmaParaEditar == null)
            return Result<TurmaDtoResponse>.Falha("Turma não encontrada para atualização.");

        var resultadoVO = CodigoTurma.Criar(dto.Sigla, dto.AnoLetivo, dto.Semestre, dto.Numero);
        if (!resultadoVO.Sucesso)
            return Result<TurmaDtoResponse>.Falha(resultadoVO.Mensagem);

        var codigoValidado = resultadoVO.Dados;

        var turmaComMesmoCodigo = await _turmaRepo.ObterPorCodigoIgnorandoFiltrosAsync(codigoValidado.ValorFormatado);

        if (turmaComMesmoCodigo != null && turmaComMesmoCodigo.Id != turmaId)
            return Result<TurmaDtoResponse>.Conflito("Este código já está sendo usado por outra turma."); // 👈 Tem que ser .Conflito!

        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<TurmaDtoResponse>.Falha("Professor não encontrado ou inativo.");
        if (!professor.Ativo)
            return Result<TurmaDtoResponse>.Conflito("Professor nao encontrado por está inativo.");

        var disciplina = await _disciplinaRepo.ObterPorIdAsync(dto.DisciplinaId);
        if (disciplina == null)
            return Result<TurmaDtoResponse>.Falha("Disciplina não encontrada ou inativa.");
        if (!disciplina.Ativo)
            return Result<TurmaDtoResponse>.Falha("Disciplina não encontrada por está inativa.");

        turmaParaEditar.AtualizarDados(codigoValidado, dto.ProfessorId, dto.DisciplinaId);

        if (dto.Ativo) turmaParaEditar.Ativar(); else turmaParaEditar.Desativar();

        await _turmaRepo.AtualizarAsync(turmaParaEditar);
        return Result<TurmaDtoResponse>.Ok(turmaParaEditar.ToTurmaDtoResponse());
    }
}