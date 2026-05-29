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

    public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, Guid notaid, NotaDtoUpdate dto)
    {
        // 1. Pedágio: Existe a matrícula?
        if (!await _uow.Matriculas.ExisteAsync(matriculaId))
            return Result<NotaDtoResponse>.NaoEncontrado("Matrícula não encontrada.");

        // 2. Busca a nota
        var nota = await _uow.Notas.ObterPorId(notaid
            );

        // 3. Pedágio: A nota existe E pertence a esta matrícula?
        if (nota is null || nota.MatriculaId != matriculaId)
            return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada para esta matrícula.");

        // 4. Aplica a atualização (Aqui é onde a regra dos 0-10 deve estar na entidade)
        try
        {
            nota.AtualizarDados(dto.Valor, dto.Descricao, dto.Importancia.Value, dto.Categoria.Value
                );
            await _uow.CommitAsync();
            return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
        }
        catch (ArgumentException ex) // Ou a sua classe de erro de negócio
        {
            return Result<NotaDtoResponse>.Falha(ex.Message); // Isso vai gerar o BadRequest
        }
    }
}