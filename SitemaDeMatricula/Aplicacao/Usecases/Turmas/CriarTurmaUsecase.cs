using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;

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
    {// 1. Tenta criar o VO
        var resultadoVO = CodigoTurma.Criar(dto.Sigla, dto.AnoLetivo, dto.Semestre, dto.Numero);

        // 2. Verifica se falhou (Usando sua propriedade Sucesso)
        if (!resultadoVO.Sucesso)
        {
            return Result<TurmaDtoResponse>.Falha(resultadoVO.Mensagem);
        }

        // 3. Se deu sucesso, acessamos o objeto real via .Dados
        var codigoValidado = resultadoVO.Dados;

        var turmaExistente = await _turmaRepo.ObterPorCodigoAsync(codigoValidado);
        if (turmaExistente != null)
            return Result<TurmaDtoResponse>.Conflito("Já existe uma turma (ativa ou inativa) com este código.");

        // 3. Validação de Dependências (Regra de Negócio)
        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<TurmaDtoResponse>.Falha("Professor não encontrado ou inativo.");

        var disciplina = await _discRepo.ObterPorIdAsync(dto.DisciplinaId);
        if (disciplina == null)
            return Result<TurmaDtoResponse>.Falha("Disciplina não encontrada ou inativa.");

        // 4. Se passou por tudo, a Entidade é criada com segurança
        var novaTurma = new Turma(codigoValidado, dto.ProfessorId, dto.DisciplinaId);

        await _turmaRepo.AdicionarAsync(novaTurma);
        return Result<TurmaDtoResponse>.Ok(novaTurma.ToTurmaDtoResponse());
    }
}