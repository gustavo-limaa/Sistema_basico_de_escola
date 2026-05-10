using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases.Estudante;

public class UsesCasesDeletarEstudante
{
    private readonly IRepositorioEstudante _repositorioEstudante;

    public UsesCasesDeletarEstudante(IRepositorioEstudante repositorioEstudante)
    {
        _repositorioEstudante = repositorioEstudante;
    }

    public async Task<Result<bool>> ExecuteAsync(Guid id)
    {
        try
        {
            var result = await _repositorioEstudante.ObterPorIdAsync(id);
            if (result is null)
                return Result<bool>.Falha("Estudante não encontrado.");

            if (result is null)
                return Result<bool>.Falha("Estudante não encontrado.");
            _repositorioEstudante.Remover(result);
            var deleteResult = await _repositorioEstudante.SalvarAlteracoesAsync();
            if (!deleteResult)
                return Result<bool>.Falha("Falha ao deletar o estudante.");
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Falha($"Erro ao deletar estudante: {ex.Message}");
        }
    }
}