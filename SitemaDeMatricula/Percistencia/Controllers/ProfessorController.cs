using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Aplicacao.Usecases.Professor;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;

namespace SitemaDeMatricula.Percistencia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessorController : ControllerBase
{
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

    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromServices] ProfessorObterTodosUsecases useCase)
    {
        var result = await useCase.ExecutarAsync();

        if (!result.Sucesso)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("cpf/{cpf}")]
    public async Task<IActionResult> ObterPorCpf([FromServices] ProfessorObterPorCpfUsecases useCase, string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return BadRequest("O CPF do professor deve ser informado.");

        var result = await useCase.ExecutarAsync(cpf);

        if (!result.Sucesso)
        {
            if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromServices] ProfessorCriarUsecases useCase, [FromBody] ProfessorDtoCreate professorDto)
    {
        if (professorDto == null)
            return BadRequest("Os dados do professor devem ser informados.");

        if (string.IsNullOrWhiteSpace(professorDto.NomeCompleto))
            return BadRequest("O nome completo do professor é obrigatório.");

        var result = await useCase.ExecutarAsync(professorDto);

        if (!result.Sucesso)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados.ProfessorId }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromServices] ProfessorAtualizarUsecase useCase, [FromBody] ProfessorDtoUpdate professorDto)
    {
        if (professorDto == null)
            return BadRequest("Os dados do professor devem ser informados.");

        if (professorDto.ProfessorId == Guid.Empty)
            return BadRequest("O ID do professor deve ser informado para atualização.");

        var result = await useCase.ExecutarAsync(professorDto);
        if (!result.Sucesso)
        {
            if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar([FromServices] ProfessorRemoverUsecase useCase, Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("O ID do professor deve ser informado para exclusão.");

        var result = await useCase.ExecutarAsync(id);
        if (!result.Sucesso)
        {
            if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);
            return BadRequest(result);
        }
        return Ok(result);
    }
}