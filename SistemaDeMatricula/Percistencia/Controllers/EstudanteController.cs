using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Percistencia.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class EstudanteController : MainController
{
    private readonly IRepositorioEstudante _repositorioEstudante;

    public EstudanteController(IRepositorioEstudante repositorioEstudante)
    {
        _repositorioEstudante = repositorioEstudante;
    }

    [HttpGet("{Id}")]
    [Authorize(Roles = "Admin,Estudante,Professor")]
    public async Task<IActionResult> ObterPorId([FromServices] UsesCasesPegarPorIdEstudante useCase, Guid id)
    {
        var result = await useCase.ExecuteAsync(id);

        if (!result.Sucesso)
        {
            if (result.Mensagem.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Mensagem);

            return BadRequest(result.Mensagem);
        }

        return Ok(result.Dados);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromServices] UsesCasesCriarEstudante useCase, [FromBody] EstudanteDtoCreate estudanteDto)
    {
        var result = await useCase.ExecuteAsync(estudanteDto);

        if (!result.Sucesso)
        {
            return BadRequest(result.Mensagem);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados.EstudanteId }, result.Dados);
    }

    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar([FromServices] UsesCasesAtualizarEstudante useCase, Guid id, [FromBody] EstudanteDtoUpdate estudanteDto)
    {
        var result = await useCase.ExecuteAsync(id, estudanteDto);

        if (!result.Sucesso)
        {
            return BadRequest(result.Mensagem);
        }

        return Ok(result.Dados);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deletar([FromServices] UsesCasesDeletarEstudante useCase, Guid id)
    {
        var result = await useCase.ExecuteAsync(id);

        if (!result.Sucesso)
        {
            return BadRequest(result.Mensagem);
        }

        return NoContent();
    }
}