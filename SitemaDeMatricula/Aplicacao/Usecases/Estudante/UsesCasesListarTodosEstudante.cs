using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases.Estudante;

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