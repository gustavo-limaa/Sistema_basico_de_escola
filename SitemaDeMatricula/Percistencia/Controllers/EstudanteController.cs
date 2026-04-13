using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Aplicacao.Usecases;
using SitemaDeMatricula.Aplicacao.Usecases.Estudante;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Percistencia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstudanteController : ControllerBase
    {
        private readonly IRepositorioEstudante _repositorioEstudante;

        public EstudanteController(IRepositorioEstudante repositorioEstudante)
        {
            _repositorioEstudante = repositorioEstudante;
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> ObterPorId([FromServices] UsesCasesPegarPorIdEstudante useCase, Guid id)
        {
            // 1. Validação básica de entrada (O porteiro checa o crachá)
            if (id == Guid.Empty)
                return BadRequest("O ID do estudante deve ser informado.");

            // 2. Chama a lógica de negócio
            var result = await useCase.ExecuteAsync(id);

            // 3. Decide a resposta baseada no Result
            if (!result.Sucesso)
            {
                // Se a falha for porque não encontrou, retornamos 404
                if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result.Mensagem);

                // Outros erros de negócio retornam 400
                return BadRequest(result.Mensagem);
            }

            // 4. Se deu tudo certo, retorna 200 com os dados
            return Ok(result.Dados);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromServices] UsesCasesCriarEstudante useCase, [FromBody] EstudanteDtoCreate estudanteDto)
        {
            if (estudanteDto == null)
                return BadRequest("Os dados do estudante devem ser informados.");

            var result = await useCase.ExecuteAsync(estudanteDto);

            if (!result.Sucesso)
            {
                return BadRequest(result.Mensagem);
            }

            return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados.EstudanteId }, result.Dados);
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos([FromServices] UsesCasesListarTodosEstudante useCase)
        {
            var result = await useCase.ExecuteAsync();

            if (!result.Sucesso)
            {
                return BadRequest(result.Mensagem);
            }
            return Ok(result.Dados);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar([FromServices] UsesCasesAtualizarEstudante useCase, Guid id, [FromBody] EstudanteDtoUpdate estudanteDto)
        {
            if (id == Guid.Empty)
                return BadRequest("O ID do estudante deve ser informado.");
            if (estudanteDto == null)
                return BadRequest("Os dados do estudante devem ser informados.");

            var result = await useCase.ExecuteAsync(id, estudanteDto);

            if (!result.Sucesso)
            {
                return BadRequest(result.Mensagem);
            }

            return Ok(result.Dados);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar([FromServices] UsesCasesDeletarEstudante useCase, Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("O ID do estudante deve ser informado.");

            var result = await useCase.ExecuteAsync(id);

            if (!result.Sucesso)
            {
                return BadRequest(result.Mensagem);
            }

            return NoContent();
        }
    }
}