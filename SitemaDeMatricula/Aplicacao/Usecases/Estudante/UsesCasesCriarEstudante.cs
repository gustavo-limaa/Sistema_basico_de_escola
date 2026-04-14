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
            await _repositorioEstudante.AdicionarAsync(novoEstudante);
            if (novoEstudante is null)
                return Result<EstudanteDtoResponse>.Falha("Falha ao criar o estudante.");

            if (novoEstudante.Cpf.Valor != dto.Cpf)
                return Result<EstudanteDtoResponse>.Falha("CPF inválido.");

            var resultRepositorio = await _repositorioEstudante.SalvarAlteracoesAsync();

            // 3. Se o repositório falhou (ex: CPF duplicado), o Use Case repassa a falha
            if (!resultRepositorio)
                return Result<EstudanteDtoResponse>.Falha("Falha ao criar o estudante.");

            // 4. Se deu certo, transforma a entidade salva (novoEstudante) em DTO de resposta
            var respostaDto = novoEstudante.ToEstudanteDtoResponse();

            return Result<EstudanteDtoResponse>.Ok(respostaDto);
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha($"Erro ao criar estudante: {ex.Message}");
        }
    }
}