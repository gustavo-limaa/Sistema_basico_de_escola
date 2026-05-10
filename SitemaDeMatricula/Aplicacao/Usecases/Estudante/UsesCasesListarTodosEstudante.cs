using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Estudante;

public sealed class UsesCasesListarTodosEstudante
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
            var result = await _repositorioEstudante.ObterTodosAsync();

            if (result is null)
                return Result<List<EstudanteDtoResponse>>.Falha("Erro ao acessar o repositório de estudantes.");

            var estudantesDto = result
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