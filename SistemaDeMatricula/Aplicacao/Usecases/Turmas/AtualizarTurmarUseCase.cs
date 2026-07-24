using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Value_Object;

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
            return Result<TurmaDtoResponse>.Falha(MensagensTurma.Invalida);

        var resultadoVO = CodigoTurma.Criar(dto.Sigla, dto.AnoLetivo, dto.Semestre, dto.Numero);
        if (!resultadoVO.Sucesso)
            return Result<TurmaDtoResponse>.Falha(resultadoVO.Mensagem);

        var codigoValidado = resultadoVO.Dados;

        var turmaComMesmoCodigo = await _turmaRepo.ObterPorCodigoIgnorandoFiltrosAsync(codigoValidado.ValorFormatado);

        if (turmaComMesmoCodigo != null && turmaComMesmoCodigo.Id != turmaId)
            return Result<TurmaDtoResponse>.Conflito(MensagensTurma.TurmaJaExistente);

        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor is null)
            return Result<TurmaDtoResponse>.Falha(MensagensProfessor.ProfessorNaoEncontrado);
        if (!professor.Ativo)
            return Result<TurmaDtoResponse>.Conflito(MensagensProfessor.ProfessorNaoEncontrado);

        var disciplina = await _disciplinaRepo.ObterPorIdAsync(dto.DisciplinaId);
        if (disciplina == null)
            return Result<TurmaDtoResponse>.Falha(MensagensDisciplina.DisciplinaNaoEncontrada);
        if (!disciplina.Ativo)
            return Result<TurmaDtoResponse>.Falha(MensagensDisciplina.DisciplinaNaoEncontrada);

        turmaParaEditar.AtualizarDados(codigoValidado, dto.ProfessorId, dto.DisciplinaId, dto.novaCapacidade);

        if (dto.Ativo) turmaParaEditar.Ativar(); else turmaParaEditar.Desativar();

        await _turmaRepo.AtualizarAsync(turmaParaEditar);
        return Result<TurmaDtoResponse>.Ok(turmaParaEditar.ToTurmaDtoResponse());
    }
}