using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Uteis;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas;

public sealed class AtualizarNotaUsecase
{
    private readonly IUnitOfWork _uow;

    public AtualizarNotaUsecase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, Guid notaid, NotaDtoUpdate Dto)
    {
        var nota = await _uow.Notas.ObterPorId(notaid);
        if (nota is null)
            return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada.");

        if (nota.MatriculaId != matriculaId)
            return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada para a matrícula informada.");

        nota.AtualizarDados(Dto.Valor, Dto.Descricao ?? nota.Descricao, Dto.Importancia ?? nota.Importancia, Dto.Categoria ?? nota.Categoria);

        await _uow.CommitAsync();
        return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
    }
}