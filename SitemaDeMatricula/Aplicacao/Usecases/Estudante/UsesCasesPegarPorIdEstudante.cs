using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases;

public class UsesCasesPegarPorIdEstudante
{
    private readonly IRepositorioEstudante _repositorioEstudante;

    public UsesCasesPegarPorIdEstudante(IRepositorioEstudante repositorioEstudante)
    {
        _repositorioEstudante = repositorioEstudante;
    }

    public async Task<Result<EstudanteDtoResponse>> ExecuteAsync(Guid id)
    {
        try
        {
            // 1. Chama o repositório
            var result = await _repositorioEstudante.ObterPorIdAsync(id);
            if (result is null)
                return Result<EstudanteDtoResponse>.Falha("Erro ao acessar o repositório de estudantes.");

            // 2. Verifica se o repositório retornou uma falha (ex: erro de banco ou estudante não encontrado)
            if (!result.Sucesso)
                return Result<EstudanteDtoResponse>.Falha(result.Mensagem);
            // 3. Mapeia a Entidade (que está dentro de result.Dados) para DTO
            var estudanteDto = result.Dados.ToEstudanteDtoResponse();
            return Result<EstudanteDtoResponse>.Ok(estudanteDto);
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha($"Erro ao obter estudante por ID: {ex.Message}");
        }
    }
}