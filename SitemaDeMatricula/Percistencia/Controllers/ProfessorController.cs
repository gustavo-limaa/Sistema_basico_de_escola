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
    // O "Coração" da limpeza: Centraliza a tradução do Result Pattern para HTTP
    private IActionResult TratarResultado<T>(Result<T> result)
    {
        // 1. Sucesso? 200 OK e tchau.
        if (result.Sucesso) return Ok(result.Dados);

        // 2. Erros específicos (Filtros)

        // Se não encontrou -> 404
        if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { mensagem = result.Mensagem });

        // Se já existe (Conflito) -> 409
        if (result.Mensagem.Contains("já existe", StringComparison.OrdinalIgnoreCase))
            return Conflict(new { mensagem = result.Mensagem });

        // 3. Caso não seja nada específico, cai no erro genérico de cliente -> 400
        return BadRequest(new { mensagem = result.Mensagem });
    }

    // Busca por ID - Curto e grosso
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId([FromServices] ProfessorObterPorIdUsecases useCase, Guid id)
        => TratarResultado(await useCase.ExecutarAsync(id));

    [HttpGet("cpf/{cpf:length(11)}")] // A rota só ativa se o CPF tiver 11 caracteres
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
        return TratarResultado(result); // Ele resolve tudo sozinho!
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