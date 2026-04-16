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

        public async Task<Result<bool>> Executar(Guid id, DisciplinaDtoUpdate dto)
        {
            // 1. O Repositório vai lá no banco perguntar: "Tem alguém com esse nome?"
            var nomeJaExiste = await _disciplinaRepositorio.ExisteDisciplinaComMesmoNomeAsync(dto.Nome);

            // 2. Se o repositório responder que SIM (true), a gente para tudo aqui
            if (nomeJaExiste)
            {
                // O seu Result.Falha é o que vai levar a mensagem até o Swagger/Controller
                return Result<bool>.Falha($"Já existe uma disciplina cadastrada com o nome '{dto.Nome}'.");
            }

            var disciplina = await _disciplinaRepositorio.ObterPorIdAsync(id);
            if (disciplina == null)
                return Result<bool>.Falha("Disciplina não encontrada.");

            // 1. Validação de Nome Inteligente
            // Só verificamos o banco se o nome que veio no DTO for DIFERENTE do nome atual
            if (dto.Nome.Trim().ToLower() != disciplina.Nome.Valor.ToLower())
            {
                if (await _disciplinaRepositorio.ExisteDisciplinaComMesmoNomeAsync(dto.Nome))
                    return Result<bool>.Falha("Já existe outra disciplina com esse nome.");
            }

            // 2. Usar o Mapper (Lembra que ele já cuida do Ativar/Desativar?)
            // Em vez de fazer na mão, chame a extensão que criamos:
            disciplina.ToAtualizarDisciplina(dto);

            // 3. Persistência
            _disciplinaRepositorio.Atualizar(disciplina);
            var resultado = await _disciplinaRepositorio.SalvarAlteracoesAsync();

            return Result<bool>.SemConteudo("Disciplina atualizada com sucesso!");
        }
    }
}