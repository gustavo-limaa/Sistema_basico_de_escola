using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Aplicacao.Usecases.Disciplinas
{
    public sealed class RemoverUseCaseDisciplina
    {
        private readonly IDisciplinaRepositorio _disciplinaRepositorio;

        public RemoverUseCaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)
        {
            _disciplinaRepositorio = disciplinaRepositorio;
        }

        public async Task<Result<DisciplinaDtoResponse>> Executar(Guid id)
        {
            var disciplina = await _disciplinaRepositorio.ObterPorIdAsync(id);

            if (disciplina == null)
                return Result<DisciplinaDtoResponse>.Falha("Disciplina não encontrada.");

            disciplina.Desativar();

            _disciplinaRepositorio.Atualizar(disciplina);

            var resultado = await _disciplinaRepositorio.SalvarAlteracoesAsync();

            return Result<DisciplinaDtoResponse>.SemConteudo("Disciplina desativada com sucesso!");
        }
    }
}