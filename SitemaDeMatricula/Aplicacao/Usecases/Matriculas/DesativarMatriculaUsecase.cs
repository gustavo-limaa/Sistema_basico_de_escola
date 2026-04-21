using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Aplicacao.Usecases.Matriculas;

public class DesativarMatriculaUsecase
{
    private readonly IRepositorioMatricula _matriculaRepo;

    public DesativarMatriculaUsecase(IRepositorioMatricula matriculaRepo)
    {
        _matriculaRepo = matriculaRepo;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var matricula = await _matriculaRepo.ObterPorIdAsync(id);
        if (matricula == null)
            return Result<bool>.Falha("Matrícula não encontrada.");
        if (!matricula.Ativo)
            return Result<bool>.Falha("Matrícula já está desativada.");
        matricula.Desativar();
        await _matriculaRepo.AtualizarAsync(matricula);
        return Result<bool>.Ok(true);
    }
}