using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Aplicacao.Usecases.Professor;
using SitemaDeMatricula.Domain;

namespace SitemaDeMatricula.Percistencia.Controllers;

[ApiController]
[Route("api/professores")]
public class ProfessorController : ControllerBase
{
    private IActionResult TratarResultado<T>(Result<T> result)
    {
        if (result.Sucesso) return Ok(result.Dados);

        if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { mensagem = result.Mensagem });

        if (result.Mensagem.Contains("já existe", StringComparison.OrdinalIgnoreCase))
            return Conflict(new { mensagem = result.Mensagem });

        return BadRequest(new { mensagem = result.Mensagem });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId([FromServices] ProfessorObterPorIdUsecases useCase, Guid id)
        => TratarResultado(await useCase.ExecutarAsync(id));

    [HttpGet("cpf/{cpf:length(11)}")]
    public async Task<IActionResult> ObterPorCpf([FromServices] ProfessorObterPorCpfUsecases useCase, string cpf)
        => TratarResultado(await useCase.ExecutarAsync(cpf));

    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromServices] ProfessorObterTodosUsecases useCase)
        => TratarResultado(await useCase.ExecutarAsync());

    [HttpPost]
    public async Task<IActionResult> Criar([FromServices] ProfessorCriarUsecases useCase, ProfessorDtoCreate professorDto)
    {
        var result = await useCase.ExecutarAsync(professorDto);

        if (!result.Sucesso)
            return TratarResultado(result);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados.ProfessorId }, result.Dados);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromServices] ProfessorAtualizarUsecase useCase, ProfessorDtoUpdate professorDto)
    {
        var result = await useCase.ExecutarAsync(professorDto);
        return TratarResultado(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar([FromServices] ProfessorRemoverUsecase useCase, Guid id)
    {
        var result = await useCase.ExecutarAsync(id);

        if (result.Sucesso) return NoContent();
        return TratarResultado(result);
    }

    [HttpPatch("{id}/restaurar")]
    public async Task<IActionResult> Restaurar([FromServices] ProfessorRestaurarUseCase useCase, Guid id)
    => TratarResultado(await useCase.ExecutarAsync(id));
}