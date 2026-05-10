using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases.Estudante;

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
            var estudante = await _repositorioEstudante.ObterPorIdAsync(id);

            if (estudante is null)
                return Result<EstudanteDtoResponse>.Falha("Estudante não encontrado.");

            var estudanteDto = estudante.ToEstudanteDtoResponse();

            return Result<EstudanteDtoResponse>.Ok(estudanteDto);
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha($"Erro ao obter estudante por ID: {ex.Message}");
        }
    }
}