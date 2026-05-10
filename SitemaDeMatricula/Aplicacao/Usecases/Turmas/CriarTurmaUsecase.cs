using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Value_Object;

namespace SistemaDeMatricula.Aplicacao.Usecases.Turmas;

public sealed class CriarTurmaUseCase
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

    public async Task<Result<TurmaDtoResponse>> ExecutarAsync(TurmaDtoCreate dto)
    {
        var resultadoVO = CodigoTurma.Criar(dto.Sigla, dto.AnoLetivo, dto.Semestre, dto.Numero);

        if (!resultadoVO.Sucesso)
        {
            return Result<TurmaDtoResponse>.Falha(resultadoVO.Mensagem);
        }

        var codigoVO = resultadoVO.Dados;
        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<TurmaDtoResponse>.Falha("Professor não encontrado.");
        if (!professor.Ativo)
            return Result<TurmaDtoResponse>.Conflito("Não é possível vincular um Professor inativo a uma nova turma.");

        var turmaExistente = await _turmaRepo.ObterPorCodigoAsync(codigoVO.ValorFormatado);

        if (turmaExistente != null)
            return Result<TurmaDtoResponse>.Conflito("Já existe uma turma ativa com este código.");

        var disciplina = await _discRepo.ObterPorIdAsync(dto.DisciplinaId);
        if (disciplina == null)
            return Result<TurmaDtoResponse>.NaoEncontrado("Disciplina não encontrada."); // Agora retorna 404

        if (!disciplina.Ativo)
            return Result<TurmaDtoResponse>.Conflito("Não é possível vincular uma disciplina inativa a uma nova turma."); // Retorna 400

        var novaTurma = new Turma(codigoVO, dto.ProfessorId, dto.DisciplinaId);
        await _turmaRepo.AdicionarAsync(novaTurma);
        return Result<TurmaDtoResponse>.Ok(novaTurma.ToTurmaDtoResponse());
    }
}