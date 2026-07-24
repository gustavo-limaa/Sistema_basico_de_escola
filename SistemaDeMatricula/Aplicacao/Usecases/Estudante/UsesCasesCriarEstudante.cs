using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Services;

namespace SistemaDeMatricula.Aplicacao.Usecases.Estudante;

public sealed class UsesCasesCriarEstudante
{
    private readonly IRepositorioEstudante _repositorioEstudante;
    private readonly IUsuarioLogadoService _usuarioLogadoService;

    public UsesCasesCriarEstudante(IRepositorioEstudante repositorioEstudante, IUsuarioLogadoService usuarioLogadoService)
    {
        _repositorioEstudante = repositorioEstudante;
        _usuarioLogadoService = usuarioLogadoService;
    }

    public async Task<Result<EstudanteDtoResponse>> ExecuteAsync(EstudanteDtoCreate dto)
    {
        try
        {
            if (dto is null)
                return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroAoCriarEstudante);

            var cpfexistente = await _repositorioEstudante.ObterPorCpfAsync(dto.Cpf);

            if (cpfexistente != null)
                return Result<EstudanteDtoResponse>.Conflito(MensagensEstudante.ErroDeDuplicidade);

            var novoEstudante = dto.ToEstudante();

            var usuarioId = _usuarioLogadoService.ObterUsuarioId();

            novoEstudante.VincularUsuario(usuarioId);

            await _repositorioEstudante.AdicionarAsync(novoEstudante);
            var resultRepositorio = await _repositorioEstudante.SalvarAlteracoesAsync();

            if (!resultRepositorio)
                return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroBanco);

            var respostaDto = novoEstudante.ToEstudanteDtoResponse();

            return Result<EstudanteDtoResponse>.Ok(respostaDto);
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha(MensagensEstudante.ErroBanco);
        }
    }
}