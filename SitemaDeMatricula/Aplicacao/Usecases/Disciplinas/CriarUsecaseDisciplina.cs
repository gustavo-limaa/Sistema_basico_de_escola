using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Disciplinas;

public class CriarUsecaseDisciplina
{
    private readonly IDisciplinaRepositorio _disciplinaRepositorio;

    public CriarUsecaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)

    {
        _disciplinaRepositorio = disciplinaRepositorio;
    }

    // 1. Mude o retorno para Result<DisciplinaDtoResponse>
    public async Task<Result<DisciplinaDtoResponse>> Executar(DisciplinaDtoCreate dto)
    {
        if (dto is null)
            return Result<DisciplinaDtoResponse>.Falha("Dados da disciplina são obrigatórios.");

        // Verificar se já existe uma disciplina com o mesmo nome
        if (await _disciplinaRepositorio.ExisteDisciplinaComMesmoNomeAsync(dto.Nome))
            return Result<DisciplinaDtoResponse>.Conflito("Já existe uma disciplina com esse nome.");

        var novaDisciplina = new Domain.Modelos.Disciplina(dto.Nome, dto.CargaHoraria);

        await _disciplinaRepositorio.AdicionarAsync(novaDisciplina);
        await _disciplinaRepositorio.SalvarAlteracoesAsync();

        // 2. Aqui está o segredo: Retornamos o DTO mapeado!
        // Usando aquele ToResponse() que você criou no Mapper
        var response = novaDisciplina.ToResponse();

        return Result<DisciplinaDtoResponse>.Ok(response);
    }
}