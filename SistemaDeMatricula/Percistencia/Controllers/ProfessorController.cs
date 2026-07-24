using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;

namespace SistemaDeMatricula.Percistencia.Controllers;

[Authorize]
[ApiController]
[Route("api/professores")]
public sealed class ProfessorController : ControllerBase
{
    private IActionResult TratarResultado<T>(Result<T> result)
    {
        if (result.Sucesso) return Ok(result.Dados);

        if (result.Mensagem == MensagensProfessor.ProfessorNaoEncontrado)
            return NotFound(new { mensagem = result.Mensagem });

        if (result.Mensagem == MensagensProfessor.ProfessorJaExiste ||
            result.Mensagem == MensagensProfessor.ErroDeDuplicidade)
            return Conflict(new { mensagem = result.Mensagem });

        if (result.Mensagem == MensagensProfessor.ErroSemAutoridade)
            return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = result.Mensagem });

        return BadRequest(new { mensagem = result.Mensagem });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Professor,Admin")]
    public async Task<IActionResult> ObterPorId([FromServices] ProfessorObterPorIdUsecases useCase, Guid id)
        => TratarResultado(await useCase.ExecutarAsync(id));

    [HttpGet("cpf/{cpf:length(11)}")]
    [Authorize(Roles = "Professor,Admin")]
    public async Task<IActionResult> ObterPorCpf([FromServices] ProfessorObterPorCpfUsecases useCase, string cpf)
        => TratarResultado(await useCase.ExecutarAsync(cpf));

    [HttpGet]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> ObterTodos([FromServices] ProfessorObterTodosUsecases useCase)
        => TratarResultado(await useCase.ExecutarAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromServices] ProfessorCriarUsecases useCase, ProfessorDtoCreate professorDto)
    {
        var result = await useCase.ExecutarAsync(professorDto);

        if (!result.Sucesso)
            return TratarResultado(result); // reaproveita a lógica de erro (Conflict/NotFound/BadRequest)

        return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados!.ProfessorId }, result.Dados);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> Atualizar([FromServices] ProfessorAtualizarUsecase useCase, ProfessorDtoUpdate professorDto)
    {
        var result = await useCase.ExecutarAsync(professorDto);
        if (!result.Sucesso)
        {
            return result.Mensagem switch
            {
                MensagensProfessor.ErroDeDuplicidade => Conflict(result.Mensagem),

                MensagensProfessor.ProfessorNaoEncontrado => NotFound(result.Mensagem),

                _ => BadRequest(result.Mensagem)
            };
        }
        if (result.Mensagem == MensagensProfessor.ErroSemAutoridade)
            return StatusCode(StatusCodes.Status403Forbidden, result.Mensagem);

        return Ok(result.Dados);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deletar([FromServices] ProfessorRemoverUsecase useCase, Guid id)
    {
        var result = await useCase.ExecutarAsync(id);

        if (result.Sucesso) return NoContent();
        return TratarResultado(result);
    }

    [HttpPatch("{id}/restaurar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restaurar([FromServices] ProfessorRestaurarUseCase useCase, Guid id)
    => TratarResultado(await useCase.ExecutarAsync(id));
}