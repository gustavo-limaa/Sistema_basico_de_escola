using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Aplicacao.Usecases.Disciplinas
{
    public class RemoverUseCaseDisciplina
    {
        private readonly IDisciplinaRepositorio _disciplinaRepositorio;

        public RemoverUseCaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)
        {
            _disciplinaRepositorio = disciplinaRepositorio;
        }

        public async Task<Result<bool>> Executar(Guid id)
        {
            // 1. Busca a disciplina
            var disciplina = await _disciplinaRepositorio.ObterPorIdAsync(id);

            if (disciplina == null)
                return Result<bool>.Falha("Disciplina não encontrada.");

            // 2. Em vez de _repo.Remover, usamos a regra de negócio da Entidade!
            disciplina.Desativar();

            // 3. O Repositório apenas avisa o EF que houve uma mudança
            _disciplinaRepositorio.Atualizar(disciplina);

            var resultado = await _disciplinaRepositorio.SalvarAlteracoesAsync();

            return Result<bool>.SemConteudo("Disciplina desativada com sucesso!");
        }
    }
}