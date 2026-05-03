using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;
using System.Runtime.CompilerServices;
using Xunit;

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

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

    public async Task<Result<TurmaDtoResponse>> ExecutarAsync(TurmaDtoCreate dto)
    {
        // 1. Tenta criar o VO
        var resultadoVO = CodigoTurma.Criar(dto.Sigla, dto.AnoLetivo, dto.Semestre, dto.Numero);

        if (!resultadoVO.Sucesso)
        {
            return Result<TurmaDtoResponse>.Falha(resultadoVO.Mensagem);
        }

        // 2. DESEMPACOTANDO: Pegamos o objeto de valor
        var codigoVO = resultadoVO.Dados;
        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<TurmaDtoResponse>.Falha("Professor não encontrado.");
        var turmaExistente = await _turmaRepo.ObterPorCodigoAsync(codigoVO.ValorFormatado);

        if (turmaExistente != null)
            // Use o método que seta o TipoErro.Conflito
            return Result<TurmaDtoResponse>.Conflito("Já existe uma turma ativa com este código.");

        if (!professor.Ativo) // Esta linha faz o seu teste de 'ProfessorInativo' passar!
            return Result<TurmaDtoResponse>.Falha("Não é possível vincular um professor inativo a uma nova turma.");

        // 3.2 Validar se a Disciplina existe E está ativa
        var disciplina = await _discRepo.ObterPorIdAsync(dto.DisciplinaId);
        if (disciplina == null)
            return Result<TurmaDtoResponse>.Falha("Disciplina não encontrada.");

        if (!disciplina.Ativo) // Esta linha faz o seu teste de 'DisciplinaInativa' passar!
            return Result<TurmaDtoResponse>.Falha("Não é possível vincular uma disciplina inativa a uma nova turma.");

        // 4. Na hora de criar a Entidade...
        var novaTurma = new Turma(codigoVO, dto.ProfessorId, dto.DisciplinaId);
        await _turmaRepo.AdicionarAsync(novaTurma);
        return Result<TurmaDtoResponse>.Ok(novaTurma.ToTurmaDtoResponse());
    }
}