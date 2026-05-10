using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SitemaDeMatricula.Aplicacao.Usecases.Estudante;

public class UsesCasesAtualizarEstudante
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
            if (dto is null) return Result<EstudanteDtoResponse>.Falha("Dados de atualização inválidos.");

            var resultBusca = await _repositorioEstudante.ObterPorIdAsync(id);
            if (resultBusca == null) return Result<EstudanteDtoResponse>.Falha("Estudante não encontrado.");

            var estudante = resultBusca;

            estudante.AtualizarDados(
                new ObjectNomeCompleto(dto.NomeCompleto),
                new ObjectEmail(dto.Email),
                new ObjectDataNascimento(dto.DataNascimento),
                new ObjectTelefone(dto.Telefone)
            );

            _repositorioEstudante.Atualizar(estudante);
            var resultUpdate = await _repositorioEstudante.SalvarAlteracoesAsync();
            if (!resultUpdate) return Result<EstudanteDtoResponse>.Falha("Falha ao atualizar o estudante.");

            return Result<EstudanteDtoResponse>.Ok(estudante.ToEstudanteDtoResponse());
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha($"Erro ao atualizar: {ex.Message}");
        }
    }
}