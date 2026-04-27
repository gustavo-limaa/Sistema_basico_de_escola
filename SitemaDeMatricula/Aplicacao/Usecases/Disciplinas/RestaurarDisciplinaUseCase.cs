using global::SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using global::SitemaDeMatricula.Domain;
using global::SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Disciplinas;

public class RestaurarUseCaseDisciplina
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
        {
            return Result<DisciplinaDtoResponse>.Falha("Disciplina desativada não encontrada.");
        }

        if (disciplina.Ativo)
            return Result<DisciplinaDtoResponse>.Falha("Esta disciplina já está ativa e não precisa ser restaurada.");

        disciplina.Ativar();

        _disciplinaRepositorio.Atualizar(disciplina);

        await _disciplinaRepositorio.SalvarAlteracoesAsync();

        return Result<DisciplinaDtoResponse>.Ok(disciplina.ToResponse());
    }
}