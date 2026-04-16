using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Aplicacao.Usecases.Disciplinas
{
    public class CriarUsecaseDisciplina
    {
        private readonly IDisciplinaRepositorio _disciplinaRepositorio;

        public CriarUsecaseDisciplina(IDisciplinaRepositorio disciplinaRepositorio)
        {
            _disciplinaRepositorio = disciplinaRepositorio;
        }

        public async Task<Result<bool>> Executar(DisciplinaDtoCreate dto)
        {
            if (dto == null)
                return Result<bool>.Falha("Dados da disciplina são obrigatórios.");
            // Verificar se já existe uma disciplina com o mesmo nome

            if (await _disciplinaRepositorio.ExisteDisciplinaComMesmoNomeAsync(dto.Nome))
                return Result<bool>.Falha("Já existe uma disciplina com esse nome.");

            var novaDisciplina = new Domain.Modelos.Disciplina(dto.Nome, dto.CargaHoraria);

            await _disciplinaRepositorio.AdicionarAsync(novaDisciplina);

            var resultado = await _disciplinaRepositorio.SalvarAlteracoesAsync();

            return Result<bool>.Ok(resultado);
        }
    }
}