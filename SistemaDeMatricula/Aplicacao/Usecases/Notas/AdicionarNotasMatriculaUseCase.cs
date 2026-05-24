using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using System.Security.AccessControl;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas;

public class AdicionarNotasMatriculaUseCase
{
    private readonly IUnitOfWork _uow;

    public AdicionarNotasMatriculaUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, NotaDtoCreate notaDtoCreate)
    {
        var matricula = await _uow.Matriculas.ObterPorIdAsync(matriculaId);
        if (matricula == null)
            return Result<NotaDtoResponse>.NaoEncontrado("Matrícula não encontrada.");

        matricula.AdicionarNota(notaDtoCreate.Valor, notaDtoCreate.Descricao, notaDtoCreate.Importancia, notaDtoCreate.Categoria);
        var notacriada = matricula.Notas.Last();

        await _uow.Matriculas.AtualizarAsync(matricula);

        await _uow.CommitAsync();

        var notaDtoResponse = notacriada.ToNotaDtoResponse();

        return Result<NotaDtoResponse>.Ok(notaDtoResponse);
    }
}