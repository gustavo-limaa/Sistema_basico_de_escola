using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Infraestrutura.Data;

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
            return Result<bool>.Falha("O identificador da matrícula é obrigatório.");

        var matricula = await _uow.Matriculas.ObterPorIdAsync(id);

        if (matricula == null)
            return Result<bool>.Falha("Matrícula não encontrada.");

        if (!matricula.Ativo)
            return Result<bool>.Falha("Matrícula já está desativada.");

        matricula.Desativar();

        await _uow.Matriculas.AtualizarAsync(matricula);

        var sucesso = await _uow.CommitAsync();

        if (!sucesso)
            return Result<bool>.Falha("Ocorreu um erro ao desativar a matrícula no banco de dados.");

        return Result<bool>.Ok(true);
    }
}