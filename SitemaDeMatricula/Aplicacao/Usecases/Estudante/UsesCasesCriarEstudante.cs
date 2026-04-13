using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases.Estudante;

public class UsesCasesCriarEstudante
{
    private readonly IRepositorioEstudante _repositorioEstudante;

    public UsesCasesCriarEstudante(IRepositorioEstudante repositorioEstudante)
    {
        _repositorioEstudante = repositorioEstudante;
    }

    public async Task<Result<EstudanteDtoResponse>> ExecuteAsync(EstudanteDtoCreate dto)
    {
        try
        {
            if (dto is null)
                return Result<EstudanteDtoResponse>.Falha("Dados de estudante são obrigatórios.");

            // 1. Transforma em Entidade
            var novoEstudante = dto.ToEstudante();

            // 2. Tenta salvar e CAPTURA o resultado do repositório
            var resultRepositorio = await _repositorioEstudante.AdicionarAsync(novoEstudante);

            // 3. Se o repositório falhou (ex: CPF duplicado), o Use Case repassa a falha
            if (!resultRepositorio.Sucesso)
                return Result<EstudanteDtoResponse>.Falha(resultRepositorio.Mensagem);

            // 4. Se deu certo, transforma a entidade salva (resultRepositorio.Dados) em DTO de resposta
            var respostaDto = resultRepositorio.Dados.ToEstudanteDtoResponse();

            return Result<EstudanteDtoResponse>.Ok(respostaDto);
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha($"Erro ao criar estudante: {ex.Message}");
        }
    }
}