using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Disciplinas;

public sealed class CriarUsecaseDisciplina
{
    private readonly IDisciplinaRepositorio _disciplinaRepositorio;

    public CriarUsecaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)

    {
        _disciplinaRepositorio = disciplinaRepositorio;
    }

    public async Task<Result<DisciplinaDtoResponse>> Executar(DisciplinaDtoCreate dto)
    {
        if (dto is null)
            return Result<DisciplinaDtoResponse>.Falha(MensagensDisciplina.DisciplinaInvalida);

        if (await _disciplinaRepositorio.ExisteDisciplinaComMesmoNomeAsync(dto.Nome))
            return Result<DisciplinaDtoResponse>.Conflito(MensagensDisciplina.DisciplinaJaExiste);

        var novaDisciplina = new Domain.Modelos.Disciplina(dto.Nome, dto.CargaHoraria);

        await _disciplinaRepositorio.AdicionarAsync(novaDisciplina);
        await _disciplinaRepositorio.SalvarAlteracoesAsync();

        var response = novaDisciplina.ToResponse();

        return Result<DisciplinaDtoResponse>.Ok(response);
    }
}