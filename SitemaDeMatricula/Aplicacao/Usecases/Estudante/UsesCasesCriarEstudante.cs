using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;
using Xunit;

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

            if (await _repositorioEstudante.ExisteCpfAsync(dto.Cpf))
                return Result<EstudanteDtoResponse>.Falha("CPF já cadastrado.");

            var novoEstudante = dto.ToEstudante();

            await _repositorioEstudante.AdicionarAsync(novoEstudante);
            var resultRepositorio = await _repositorioEstudante.SalvarAlteracoesAsync();

            if (!resultRepositorio)
                return Result<EstudanteDtoResponse>.Falha("Falha ao salvar no banco de dados.");

            var respostaDto = novoEstudante.ToEstudanteDtoResponse();
            return Result<EstudanteDtoResponse>.Ok(respostaDto);
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha($"Erro ao criar estudante: {ex.Message}");
        }
    }
}