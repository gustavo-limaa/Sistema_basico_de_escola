using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
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
        // 1. Validação do Value Object (CodigoTurma)
        var resultadoVO = CodigoTurma.Criar(dto.Sigla, dto.AnoLetivo, dto.Semestre, dto.Numero);
        if (!resultadoVO.Sucesso)
            return Result<TurmaDtoResponse>.Falha(resultadoVO.Mensagem);

        var codigoVO = resultadoVO.Dados;

        // 2. Validação do Professor
        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<TurmaDtoResponse>.Falha(MensagensProfessor.ProfessorNaoEncontrado);

        if (!professor.Ativo)
            return Result<TurmaDtoResponse>.Falha(MensagensProfessor.ErroInativo_ou_Ativo);

        // 3. Validação de Duplicidade da Turma (Conflito real)
        var turmaExistente = await _turmaRepo.ObterPorCodigoAsync(codigoVO.ValorFormatado);
        if (turmaExistente != null)
            return Result<TurmaDtoResponse>.Conflito(MensagensTurma.TurmaJaExistente);

        // 4. Validação da Disciplina
        var disciplina = await _discRepo.ObterPorIdAsync(dto.DisciplinaId);
        if (disciplina == null)
            return Result<TurmaDtoResponse>.Falha(MensagensDisciplina.DisciplinaNaoEncontrada);

        if (!disciplina.Ativo)
            return Result<TurmaDtoResponse>.Falha(MensagensDisciplina.DisciplinaInativa);

        // 5. Criação e Persistência da Turma
        var novaTurma = new Turma(codigoVO, dto.ProfessorId, dto.DisciplinaId, dto.CapacidadeMaxima);

        await _turmaRepo.AdicionarAsync(novaTurma);

        // 🎯 O PULO DO GATO: Salva as alterações no MySQL!
        var sucessoPersistencia = await _turmaRepo.SalvarAlteracoesAsync();
        if (!sucessoPersistencia)
            return Result<TurmaDtoResponse>.Falha(MensagensTurma.ErroPersistenciaBanco);

        return Result<TurmaDtoResponse>.Ok(novaTurma.ToTurmaDtoResponse());
    }
}