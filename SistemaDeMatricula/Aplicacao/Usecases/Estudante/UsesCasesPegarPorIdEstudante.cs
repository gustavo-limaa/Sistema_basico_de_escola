using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Estudante;

public sealed class UsesCasesPegarPorIdEstudante
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
                return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroEstudanteNaoEncontrado);

            var estudanteDto = estudante.ToEstudanteDtoResponse();

            return Result<EstudanteDtoResponse>.Ok(estudanteDto);
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroBanco);
        }
    }
}