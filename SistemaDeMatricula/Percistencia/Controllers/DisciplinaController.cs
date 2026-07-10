using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Usecases.Disciplinas;

namespace SistemaDeMatricula.Percistencia.Controllers;

[Authorize]
[Route("api/disciplinas")]
public sealed class DisciplinaController : MainController
{
    [HttpPost]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> Criar([FromBody] DisciplinaDtoCreate dto, [FromServices] CriarUsecaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(dto);

        if (!resultado.Sucesso)
            return BadRequest(resultado.Mensagem);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado!.Dados!.DisciplinaId }, resultado!.Dados);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Professor,Estudante")]
    public async Task<IActionResult> ObterPorId(Guid id, [FromServices] ObterPorIdUsecaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);
        return resultado.Sucesso ? Ok(resultado.Dados) : NotFound(resultado.Mensagem);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObterTodas([FromServices] ObterTodasDisciplinaUseCase useCase)
    {
        var resultado = await useCase.Executar();

        if (!resultado.Sucesso)
            return BadRequest(resultado.Mensagem);

        return resultado.Dados.Any()
            ? Ok(resultado.Dados)
            : NoContent();
    }

    [Authorize(Roles = "Admin,Professor")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] DisciplinaDtoUpdate dto, [FromServices] AtualizarUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id, dto);

        if (!resultado.Sucesso)
        {
            return resultado.Mensagem.Contains("não encontrada")
                ? NotFound(resultado.Mensagem)
                : BadRequest(resultado.Mensagem);
        }
        return Ok(resultado.Dados);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id, [FromServices] RemoverUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);

        if (resultado.Sucesso) return NoContent();

        return resultado.Mensagem.Contains("não encontrada")
            ? NotFound(resultado.Mensagem)
            : BadRequest(resultado.Mensagem);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/restaurar")]
    public async Task<IActionResult> Restaurar(Guid id, [FromServices] RestaurarUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);

        if (resultado.Sucesso)
            return Ok(resultado.Dados);

        if (resultado.Mensagem.Contains("Esta disciplina já está ativa e não precisa ser restaurada."))
            return Conflict(resultado.Mensagem);

        if (resultado.Mensagem.Contains("Disciplina desativada não encontrada."))
            return NotFound(resultado.Mensagem);

        return BadRequest(resultado.Mensagem);
    }
}