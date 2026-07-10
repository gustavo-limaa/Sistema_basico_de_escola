using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

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
                return Result<List<EstudanteDtoResponse>>.Falha(MensagensEstudante.ErroEstudanteNaoEncontrado);

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