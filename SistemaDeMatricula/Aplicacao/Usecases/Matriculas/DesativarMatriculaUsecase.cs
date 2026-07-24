using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

public sealed class DesativarMatriculaUsecase
{
    private readonly IUnitOfWork _uow;

    public DesativarMatriculaUsecase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        if (id == Guid.Empty)
            return Result<bool>.Falha(MensagensMatricula.MatriculaNaoEncontrada);

        var matricula = await _uow.Matriculas.ObterPorIdAsync(id);

        if (matricula == null)
            return Result<bool>.NaoEncontrado(MensagensMatricula.MatriculaNaoEncontrada);

        if (!matricula.Ativo)
            return Result<bool>.Falha(MensagensMatricula.MatriculaJaDesativada);

        matricula.Desativar();

        await _uow.Matriculas.AtualizarAsync(matricula);
        try
        {
            var sucesso = await _uow.CommitAsync();
            if (!sucesso)
                return Result<bool>.Falha(MensagensMatricula.ErroPersistenciaBanco);

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Falha($"Erro técnico: {ex.Message} | {ex.InnerException?.Message}");
        }
    }
}