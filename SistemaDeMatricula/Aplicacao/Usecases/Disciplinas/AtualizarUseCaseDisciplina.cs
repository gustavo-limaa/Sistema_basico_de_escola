using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Disciplinas;

public sealed class AtualizarUseCaseDisciplina
{
    private readonly IDisciplinaRepositorio _disciplinaRepositorio;

    public AtualizarUseCaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)
    {
        _disciplinaRepositorio = disciplinaRepositorio;
    }

    public async Task<Result<DisciplinaDtoResponse>> Executar(Guid id, DisciplinaDtoUpdate dto)
    {
        var disciplina = await _disciplinaRepositorio.ObterPorIdAsync(id);
        if (disciplina == null)
            return Result<DisciplinaDtoResponse>.Falha(MensagensDisciplina.DisciplinaNaoEncontrada);

        if (dto.Nome.Trim().ToLower() != disciplina.Nome.Valor.ToLower())
        {
            if (await _disciplinaRepositorio.ExisteDisciplinaComMesmoNomeAsync(dto.Nome))
                return Result<DisciplinaDtoResponse>.Falha(MensagensDisciplina.DisciplinaJaExiste);
        }

        disciplina.ToAtualizarDisciplina(dto);
        _disciplinaRepositorio.Atualizar(disciplina);

        var salvou = await _disciplinaRepositorio.SalvarAlteracoesAsync();

        if (!salvou)
            return Result<DisciplinaDtoResponse>.Falha(MensagensDisciplina.ErroDoBancoDeDados);

        return Result<DisciplinaDtoResponse>.Ok(disciplina.ToResponse());
    }
}