using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Professor;

public class ProfessorObterTodosUsecases
{
    private readonly IRepositorioProfessor _repositorioProfessor;

    public ProfessorObterTodosUsecases(IRepositorioProfessor repositorioProfessor)
    {
        _repositorioProfessor = repositorioProfessor;
    }

    public async Task<Result<IEnumerable<ProfessorDtoResponse>>> ExecutarAsync()
    {
        // 1. Busca no banco (Repositório já usa AsNoTracking, então é rápido)
        var professores = await _repositorioProfessor.ObterTodosAsync();

        // 2. Mapeamento
        // Se 'professores' for vazio, o .Select não quebra, ele apenas gera uma lista vazia.
        var professoresDto = professores.Select(p => p.ToProfessorDtoResponse()).ToList();

        // 3. Retorno de Sucesso (com a lista cheia ou vazia)
        return Result<IEnumerable<ProfessorDtoResponse>>.Ok(professoresDto);
    }
}