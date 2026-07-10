using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Domain.Erros;

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
            // LOGUE O ERRO REAL AQUI
            // Exemplo: ex.InnerException?.Message traz a causa raiz (ex: violação de FK)
            return Result<bool>.Falha($"Erro técnico: {ex.Message} | {ex.InnerException?.Message}");
        }
    }
}