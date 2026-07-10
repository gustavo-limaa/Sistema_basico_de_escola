using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Services;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

public sealed class ProfessorCriarUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;
    private readonly IUsuarioLogadoService _usuarioLogadoService;

    public ProfessorCriarUsecases(IRepositorioProfessor repositorioProfessor, IUsuarioLogadoService usuarioLogadoService)
    {
        _repositorioProfessor = repositorioProfessor;
        _usuarioLogadoService = usuarioLogadoService;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(ProfessorDtoCreate dto)
    {
        if (dto == null) return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorInvalido);

        try
        {
            var professor = dto.ToProfessor();

            var usuarioId = _usuarioLogadoService.ObterUsuarioId();

            professor.VincularUsuario(usuarioId);

            var professorExistenteCpf = await _repositorioProfessor.ObterPorCpfAsync(dto.Cpf);
            if (professorExistenteCpf != null)
            {
                if (professorExistenteCpf.Ativo)
                {
                    return Result<ProfessorDtoResponse>.Conflito(MensagensProfessor.ProfessorJaExiste);
                }

                return Result<ProfessorDtoResponse>.Conflito(MensagensProfessor.ProfessorNaoPodeSerAdicionado);
            }

            var professorExistenteEmail = await _repositorioProfessor.ObterPorEmailAsync(dto.Email);
            if (professorExistenteEmail != null)
                return Result<ProfessorDtoResponse>.Conflito(MensagensProfessor.ProfessorNaoPodeTerEmailInvalido);
            await _repositorioProfessor.AdicionarAsync(professor);

            var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();

            return sucesso
                ? Result<ProfessorDtoResponse>.Ok(professor.ToProfessorDtoResponse())
                : Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorNaoPodeSerAdicionado);
        }
        catch (ArgumentException ex)
        {
            return Result<ProfessorDtoResponse>.Falha(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProfessorDtoResponse>.Falha($"Erro inesperado: {ex.Message}");
        }
    }
}