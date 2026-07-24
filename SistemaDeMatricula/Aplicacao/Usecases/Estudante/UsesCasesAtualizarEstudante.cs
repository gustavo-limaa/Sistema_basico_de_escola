using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SistemaDeMatricula.Aplicacao.Usecases.Estudante;

public sealed class UsesCasesAtualizarEstudante
{
    private readonly IRepositorioEstudante _repositorioEstudante;

    public UsesCasesAtualizarEstudante(IRepositorioEstudante repositorioEstudante)
    {
        _repositorioEstudante = repositorioEstudante;
    }

    public async Task<Result<EstudanteDtoResponse>> ExecuteAsync(Guid id, EstudanteDtoUpdate dto)
    {
        try
        {
            if (dto is null) return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroAoCriarEstudante);

            var resultBusca = await _repositorioEstudante.ObterPorIdAsync(id);
            if (resultBusca == null) return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroEstudanteNaoEncontrado);

            var emailJaExiste = await _repositorioEstudante.ExisteEmailAsync(dto.Email, id);
            if (emailJaExiste)
                return Result<EstudanteDtoResponse>.Conflito(MensagensEstudante.ErroDeDuplicidade);

            var estudante = resultBusca;

            estudante.AtualizarDados(
                new ObjectNomeCompleto(dto.NomeCompleto),
                new ObjectEmail(dto.Email),
                new ObjectDataNascimento(dto.DataNascimento),
                new ObjectTelefone(dto.Telefone)
            );

            _repositorioEstudante.Atualizar(estudante);
            var resultUpdate = await _repositorioEstudante.SalvarAlteracoesAsync();
            if (!resultUpdate) return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroBanco);

            return Result<EstudanteDtoResponse>.Ok(estudante.ToEstudanteDtoResponse());
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroBanco);
        }
    }
}