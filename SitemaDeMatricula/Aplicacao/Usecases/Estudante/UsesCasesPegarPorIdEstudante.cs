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
            // 1. Busca o estudante no banco
            var estudante = await _repositorioEstudante.ObterPorIdAsync(id);

            // 2. Se não encontrou, mata o processo aqui (Fail-First)
            if (estudante is null)
                return Result<EstudanteDtoResponse>.Falha("Estudante não encontrado.");

            // 3. Se passou pelo IF, o C# sabe que 'estudante' existe.
            // Agora é só mapear e retornar sucesso!
            var estudanteDto = estudante.ToEstudanteDtoResponse();

            return Result<EstudanteDtoResponse>.Ok(estudanteDto);
        }
        catch (Exception ex)
        {
            // Aqui é onde entram os erros reais de banco ou conexão
            return Result<EstudanteDtoResponse>.Falha($"Erro ao obter estudante por ID: {ex.Message}");
        }
    }
}