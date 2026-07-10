using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
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

    public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, Guid notaid, NotaDtoUpdate dto)
    {
        if (!await _uow.Matriculas.ExisteAsync(matriculaId))
            return Result<NotaDtoResponse>.NaoEncontrado(MensagensMatricula.MatriculaNaoEncontrada);

        var nota = await _uow.Notas.ObterPorId(notaid
            );

        if (nota is null || nota.MatriculaId != matriculaId)
            return Result<NotaDtoResponse>.NaoEncontrado(MensagensNotas.NotaNaoEncontrada);

        try
        {
            nota.AtualizarDados(dto.Valor, dto.Descricao, dto.Importancia.Value, dto.Categoria.Value
                );
            await _uow.CommitAsync();
            return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
        }
        catch (ArgumentException ex)
        {
            return Result<NotaDtoResponse>.Falha(ex.Message);
        }
    }
}