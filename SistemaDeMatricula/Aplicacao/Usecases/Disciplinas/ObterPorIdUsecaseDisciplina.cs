using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Disciplinas
{
    public sealed class ObterPorIdUsecaseDisciplina
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