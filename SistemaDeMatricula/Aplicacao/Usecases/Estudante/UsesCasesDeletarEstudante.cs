using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Estudante;

public sealed class UsesCasesDeletarEstudante
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
                return Result<bool>.NaoEncontrado
                    (MensagensEstudante.ErroEstudanteNaoEncontrado);

            _repositorioEstudante.Remover
                (result);
            var deleteResult = await _repositorioEstudante.SalvarAlteracoesAsync();
            if (!deleteResult)
                return Result<bool>.Falha(MensagensEstudante.ErroBanco);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Falha($"Erro ao deletar estudante: {ex.Message}");
        }
    }
}