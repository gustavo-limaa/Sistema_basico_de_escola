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
public sealed class EstudanteController : ControllerBase
{
    private readonly IRepositorioEstudante _repositorioEstudante;

    public EstudanteController(IRepositorioEstudante repositorioEstudante)
    {
        _repositorioEstudante = repositorioEstudante;
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> ObterPorId([FromServices] UsesCasesPegarPorIdEstudante useCase, Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("O ID do estudante deve ser informado.");

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
    public async Task<IActionResult> Criar([FromServices] UsesCasesCriarEstudante useCase, [FromBody] EstudanteDtoCreate estudanteDto)
    {
        if (estudanteDto == null)
            return BadRequest("Os dados do estudante devem ser informados.");
        if (await _repositorioEstudante.ExisteCpfAsync(estudanteDto.Cpf))
        {
            return Conflict("Já existe um estudante cadastrado com este CPF.");
        }

        var result = await useCase.ExecuteAsync(estudanteDto);

        if (!result.Sucesso)
        {
            return BadRequest(result.Mensagem);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados.EstudanteId }, result.Dados);
    }

    [AllowAnonymous]
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
        if (!await _repositorioEstudante.ExisteMatriculaAsync(id))
        {
            return NotFound("Estudante não encontrado para o ID fornecido.");
        }
        if (estudanteDto.Email != null && await _repositorioEstudante.ExisteEmailAsync(estudanteDto.Email, id))
        {
            return Conflict("Já existe um estudante cadastrado com este e-mail.");
        }

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
            return NotFound("O ID do estudante deve ser informado.");
        if (!await _repositorioEstudante.ExisteMatriculaAsync(id))
        {
            return NotFound("Estudante não encontrado para o ID fornecido.");
        }

        var result = await useCase.ExecuteAsync(id);

        if (!result.Sucesso)
        {
            return BadRequest(result.Mensagem);
        }

        return NoContent();
    }
}