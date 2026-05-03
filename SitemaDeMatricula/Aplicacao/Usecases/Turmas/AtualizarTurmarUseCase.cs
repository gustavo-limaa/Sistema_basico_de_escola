using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Value_Object; // Certifique-se de importar seu Mapper

namespace SitemaDeMatricula.Aplicacao.Usecases.Turmas;

public class AtualizarTurmaUseCase
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
        // 1. Buscamos a turma que queremos editar (usando o detector de fantasmas)
        var turmaParaEditar = await _turmaRepo.ObterPorIdIgnorandoFiltrosAsync(turmaId);

        if (turmaParaEditar == null)
            return Result<TurmaDtoResponse>.Falha("Turma não encontrada para atualização.");

        // 2. Criamos o VO do novo código
        var resultadoVO = CodigoTurma.Criar(dto.Sigla, dto.AnoLetivo, dto.Semestre, dto.Numero);
        if (!resultadoVO.Sucesso)
            return Result<TurmaDtoResponse>.Falha(resultadoVO.Mensagem);

        var codigoValidado = resultadoVO.Dados;

        // 3. VALIDAR CONFLITO DE CÓDIGO (A sacada!)
        var turmaComMesmoCodigo = await _turmaRepo.ObterPorCodigoIgnorandoFiltrosAsync(codigoValidado.ValorFormatado);

        // Se achou alguém E esse alguém não é a turma que estou editando agora...
        if (turmaComMesmoCodigo != null && turmaComMesmoCodigo.TurmaId != turmaId)
            return Result<TurmaDtoResponse>.Conflito("Este código já está sendo usado por outra turma."); // 👈 Tem que ser .Conflito!

        // 4. Validação de Professor e Disciplina (Igual ao Criar)
        var professor = await _profRepo.ObterPorIdAsync(dto.ProfessorId);
        if (professor == null)
            return Result<TurmaDtoResponse>.Falha("Professor não encontrado ou inativo.");

        var disciplina = await _disciplinaRepo.ObterPorIdAsync(dto.DisciplinaId);
        if (disciplina == null)
            return Result<TurmaDtoResponse>.Falha("Disciplina não encontrada ou inativa.");

        // 5. Atualiza e Persiste
        turmaParaEditar.AtualizarDados(codigoValidado, dto.ProfessorId, dto.DisciplinaId);

        // Se o DTO de Update trouxe o Ativo, podemos atualizar o estado aqui também
        if (dto.Ativo) turmaParaEditar.Ativar(); else turmaParaEditar.Desativar();

        await _turmaRepo.AtualizarAsync(turmaParaEditar);
        return Result<TurmaDtoResponse>.Ok(turmaParaEditar.ToTurmaDtoResponse());
    }
}