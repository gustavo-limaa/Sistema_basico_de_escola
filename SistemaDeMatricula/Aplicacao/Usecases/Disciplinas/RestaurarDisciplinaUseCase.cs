using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Disciplinas;

public sealed class RestaurarUseCaseDisciplina
{
    private readonly IDisciplinaRepositorio _disciplinaRepositorio;

    public RestaurarUseCaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)
    {
        _disciplinaRepositorio = disciplinaRepositorio;
    }

    public async Task<Result<DisciplinaDtoResponse>> Executar(Guid id)
    {
        var disciplina = await _disciplinaRepositorio.ObterDesativadaPorIdAsync(id);

        if (disciplina is null)
            return Result<DisciplinaDtoResponse>.Falha(MensagensDisciplina.DisciplinaNaoEncontrada);

        if (disciplina.Ativo)
            return Result<DisciplinaDtoResponse>.Conflito(MensagensDisciplina.DisciplinaAtiva);

        disciplina.Ativar();
        _disciplinaRepositorio.Atualizar(disciplina);
        await _disciplinaRepositorio.SalvarAlteracoesAsync();

        return Result<DisciplinaDtoResponse>.Ok(disciplina.ToResponse());
    }
}