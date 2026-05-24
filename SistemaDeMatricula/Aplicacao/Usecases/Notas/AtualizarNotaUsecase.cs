using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Uteis;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas;

public class AtualizarNotaUsecase
{
    private readonly IUnitOfWork _uow;

    public AtualizarNotaUsecase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid id, Guid notaid, NotaDtoUpdate Dto)
    {
        var matricula = await _uow.Matriculas.ObterPorIdAsync(id);
        if (matricula == null)
            return Result<NotaDtoResponse>.NaoEncontrado("Matrícula não encontrada.");

        var notaExistente = matricula.Notas.FirstOrDefault(n => n.Id == notaid);
        if (notaExistente == null)
            return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada.");

        notaExistente.AtualizarDados(Dto.Valor, Dto.Descricao ?? notaExistente.Descricao, Dto.Importancia ?? notaExistente.Importancia, Dto.Categoria ?? notaExistente.Categoria);

        await _uow.CommitAsync();
        return Result<NotaDtoResponse>.Ok(notaExistente.ToNotaDtoResponse());
    }
}