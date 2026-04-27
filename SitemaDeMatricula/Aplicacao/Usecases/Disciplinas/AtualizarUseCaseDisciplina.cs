using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;

namespace SitemaDeMatricula.Aplicacao.Usecases.Disciplinas
{
    public class AtualizarUseCaseDisciplina
    {
        private readonly IDisciplinaRepositorio _disciplinaRepositorio;

        public AtualizarUseCaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)
        {
            _disciplinaRepositorio = disciplinaRepositorio;
        }

        // 1. Mude o retorno para Result<DisciplinaDtoResponse>
        public async Task<Result<DisciplinaDtoResponse>> Executar(Guid id, DisciplinaDtoUpdate dto)
        {
            var disciplina = await _disciplinaRepositorio.ObterPorIdAsync(id);
            if (disciplina == null)
                return Result<DisciplinaDtoResponse>.Falha("Disciplina não encontrada.");

            // 2. Validação de Nome (Removi a checagem duplicada que estava no topo)
            if (dto.Nome.Trim().ToLower() != disciplina.Nome.Valor.ToLower())
            {
                if (await _disciplinaRepositorio.ExisteDisciplinaComMesmoNomeAsync(dto.Nome))
                    return Result<DisciplinaDtoResponse>.Falha("Já existe outra disciplina com esse nome.");
            }

            // 3. Atualiza os dados
            disciplina.ToAtualizarDisciplina(dto);
            _disciplinaRepositorio.Atualizar(disciplina);

            var salvou = await _disciplinaRepositorio.SalvarAlteracoesAsync();

            if (!salvou)
                return Result<DisciplinaDtoResponse>.Falha("Erro ao persistir os dados.");

            // 4. RETORNO CORRETO: Mapeia a entidade atualizada para o DTO de resposta
            return Result<DisciplinaDtoResponse>.Ok(disciplina.ToResponse());
        }
    }
}