using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Estudante;

public sealed class UsecaseVerificarCpfEstudante
{
    private readonly IRepositorioEstudante _repositorio;

    public UsecaseVerificarCpfEstudante(IRepositorioEstudante repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Result<bool>> Executar(string cpf)
    {
        var cpfLimpo = cpf.Replace(".", "").Replace("-", "");

        var existe = await _repositorio.ExisteCpfAsync(cpfLimpo);

        if (!existe)
        {
            return Result<bool>.Falha(MensagensEstudante.ErroEstudanteNaoEncontrado);
        }

        return Result<bool>.Ok(true, MensagensEstudante.EstudanteJaExiste);
    }
};