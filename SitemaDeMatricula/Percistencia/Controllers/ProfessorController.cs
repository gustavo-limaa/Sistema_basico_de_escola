using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Usecases.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Percistencia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorController : ControllerBase
    {
        private readonly IRepositorioProfessor _repositorioProfessor;

        public ProfessorController(IRepositorioProfessor repositorioProfessor)
        {
            _repositorioProfessor = repositorioProfessor;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId([FromServices] ProfessorObterPorIdUsecases useCase, Guid id)
        {
            // 1. O Controller faz uma validação básica de entrada (opcional, mas bom)
            if (id == Guid.Empty)
                return BadRequest("O ID do professor deve ser informado.");

            // 2. Chama o Use Case
            var result = await useCase.ExecutarAsync(id);

            // 3. Tradução do Result Pattern para HTTP
            if (!result.Sucesso)
            {
                // Se a mensagem diz que não encontrou, mandamos 404
                if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);

                // Caso contrário, erro de requisição 400
                return BadRequest(result);
            }

            // 4. Se deu certo, 200 OK com os dados
            return Ok(result);
        }
    }
}