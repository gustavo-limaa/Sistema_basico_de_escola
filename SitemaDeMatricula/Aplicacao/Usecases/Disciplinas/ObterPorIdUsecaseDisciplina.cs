using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Disciplinas
{
    public class ObterPorIdUsecaseDisciplina
    {
        private readonly IDisciplinaRepositorio _disciplinaRepositorio;

        public ObterPorIdUsecaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)
        {
            _disciplinaRepositorio = disciplinaRepositorio;
        }

        public async Task<Result<DisciplinaDtoResponse>> Executar(Guid id)
        {
            if (id == Guid.Empty)
                return Result<DisciplinaDtoResponse>.Falha("ID da disciplina é inválido.");

            var disciplina = await _disciplinaRepositorio.ObterPorIdAsync(id);

            if (disciplina == null)
                return Result<DisciplinaDtoResponse>.Falha("Disciplina não encontrada.");

            return Result<DisciplinaDtoResponse>.Ok(disciplina.ToResponse());
        }
    }
}