using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SitemaDeMatricula.Aplicacao.Usecases;

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
            // 1. Busca o estudante existente no banco
            var resultBusca = await _repositorioEstudante.ObterPorIdAsync(id);
            if (!resultBusca.Sucesso) return Result<EstudanteDtoResponse>.Falha(resultBusca.Mensagem);

            var estudante = resultBusca.Dados;

            // 2. Usa o método da ENTIDADE para atualizar os campos (Regra de Negócio)
            // Lembra que criamos o 'AtualizarDados' lá no começo?
            estudante.AtualizarDados(
                new ObjectNomeCompleto(dto.NomeCompleto),
                new ObjectEmail(dto.Email),
                new ObjectDataNascimento(dto.DataNascimento),
                new ObjectTelefone(dto.Telefone)
            );

            // 3. Persiste a mudança no banco
            var resultUpdate = await _repositorioEstudante.AtualizarAsync(estudante);
            if (!resultUpdate.Sucesso) return Result<EstudanteDtoResponse>.Falha(resultUpdate.Mensagem);

            // 4. Retorna o DTO de resposta
            return Result<EstudanteDtoResponse>.Ok(resultUpdate.Dados.ToEstudanteDtoResponse());
        }
        catch (Exception ex)
        {
            return Result<EstudanteDtoResponse>.Falha($"Erro ao atualizar: {ex.Message}");
        }
    }
}