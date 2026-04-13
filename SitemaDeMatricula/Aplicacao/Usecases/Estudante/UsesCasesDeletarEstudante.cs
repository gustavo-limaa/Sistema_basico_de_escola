using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases;

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
            // 1. Chama o repositório para deletar
            var result = await _repositorioEstudante.ObterPorIdAsync(id);
            if (result is null)
                return Result<bool>.Falha("Erro ao acessar o repositório de estudantes.");

            // 2. Verifica se o repositório retornou uma falha (ex: erro de banco ou estudante não encontrado)
            if (!result.Sucesso)
                return Result<bool>.Falha(result.Mensagem);
            // 3. Chama o repositório para deletar
            var deleteResult = await _repositorioEstudante.RemoverAsync(id);
            if (!deleteResult.Sucesso)
                return Result<bool>.Falha(deleteResult.Mensagem);
            // 4. Retorna true se deletou com sucesso
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Falha($"Erro ao deletar estudante: {ex.Message}");
        }
    }
}