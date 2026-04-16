using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SitemaDeMatricula.Aplicacao.Usecases.Disciplinas;

namespace SitemaDeMatricula.Percistencia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisciplinaController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] DisciplinaDtoCreate dto, [FromServices] CriarUsecaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(dto);
        if (!resultado.Sucesso) return BadRequest(resultado.Mensagem);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados }, resultado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id, [FromServices] ObterPorIdUsecaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado.Mensagem);
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodas([FromServices] ObterTodasDisciplinaUseCase useCase)
    {
        var resultado = await useCase.Executar();
        return Ok(resultado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] DisciplinaDtoUpdate dto, [FromServices] AtualizarUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id, dto);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado.Mensagem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id, [FromServices] RemoverUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Mensagem);
    }
}