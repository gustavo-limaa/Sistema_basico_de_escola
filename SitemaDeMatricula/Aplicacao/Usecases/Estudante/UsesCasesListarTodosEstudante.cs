using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases;

public class UsesCasesListarTodosEstudante
{
    private readonly IRepositorioEstudante _repositorioEstudante;

    public UsesCasesListarTodosEstudante(IRepositorioEstudante repositorioEstudante)
    {
        _repositorioEstudante = repositorioEstudante;
    }

    public async Task<Result<List<EstudanteDtoResponse>>> ExecuteAsync()
    {
        try
        {
            // 1. Chama o repositório
            var result = await _repositorioEstudante.ObterTodosAsync();
            if (result is null)
                return Result<List<EstudanteDtoResponse>>.Falha("Erro ao acessar o repositório de estudantes.");

            // 2. Verifica se o repositório retornou uma falha (ex: erro de banco)
            if (!result.Sucesso)
                return Result<List<EstudanteDtoResponse>>.Falha(result.Mensagem);

            // 3. Mapeia a lista de Entidades (que está dentro de result.Dados) para DTOs
            // Use o .Select (do LINQ) para transformar cada item
            var estudantesDto = result.Dados
                .Select(e => e.ToEstudanteDtoResponse())
                .ToList();

            return Result<List<EstudanteDtoResponse>>.Ok(estudantesDto);
        }
        catch (Exception ex)
        {
            return Result<List<EstudanteDtoResponse>>.Falha($"Erro ao listar estudantes: {ex.Message}");
        }
    }
}