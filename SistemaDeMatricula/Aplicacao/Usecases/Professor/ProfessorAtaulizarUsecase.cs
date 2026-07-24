using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Services;

namespace SistemaDeMatricula.Aplicacao.Usecases.Professor;

public sealed class ProfessorAtualizarUsecase
{
    private readonly IRepositorioProfessor _repositorioProfessor;
    private readonly IUsuarioLogadoService _usuarioLogadoService;

    public ProfessorAtualizarUsecase(IRepositorioProfessor repositorioProfessor, IUsuarioLogadoService usuarioLogadoService)
    {
        _repositorioProfessor = repositorioProfessor;
        _usuarioLogadoService = usuarioLogadoService;
    }

    public async Task<Result<ProfessorDtoResponse>> ExecutarAsync(ProfessorDtoUpdate professorDto)
    {
        if (professorDto == null || professorDto.ProfessorId == Guid.Empty)
            return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorInvalido);

        var professorExistente = await _repositorioProfessor.ObterPorIdAsync(professorDto.ProfessorId);
        if (professorExistente == null)
            return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorNaoEncontrado);

        var usuarioIdLogado = _usuarioLogadoService.ObterUsuarioId();
        var ehAdmin = _usuarioLogadoService.Ehadmin();

        if (!ehAdmin && professorExistente.UsuarioId != usuarioIdLogado)
            return Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ErroSemAutoridade);

        var professorComMesmoEmail = await _repositorioProfessor.ObterPorEmailAsync(professorDto.Email);

        if (professorComMesmoEmail != null && professorComMesmoEmail.Id != professorDto.ProfessorId)
        {
            return Result<ProfessorDtoResponse>.Conflito(MensagensProfessor.ErroDeDuplicidade); // ajusta pro nome real da sua constante
        }
        professorExistente.ToAtualizarProfessor(professorDto);

        _repositorioProfessor.Atualizar(professorExistente);
        var sucesso = await _repositorioProfessor.SalvarAlteracoesAsync();
        return sucesso ? Result<ProfessorDtoResponse>.Ok(professorExistente.ToProfessorDtoResponse()) : Result<ProfessorDtoResponse>.Falha(MensagensProfessor.ProfessorInvalido);
    }
}