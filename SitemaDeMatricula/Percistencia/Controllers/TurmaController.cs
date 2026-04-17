using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Dtos.turma;

using SitemaDeMatricula.Aplicacao.Usecases.Turmas;

namespace SitemaDeMatricula.Percistencia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TurmasController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromServices] ListarTurmaUsecase useCase)
    {
        var result = await useCase.ExecutarAsync();
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Mensagem);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, [FromServices] ObterPorIdTurma useCase)
    {
        var result = await useCase.ExecutarAsync(id);
        return result.Sucesso ? Ok(result.Dados) : NotFound(result.Mensagem);
    }

    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> ObterPorCodigo(string codigo, [FromServices] ObterPorCodigoTurma useCase)
    {
        var result = await useCase.ExecutarAsync(codigo);
        return result.Sucesso ? Ok(result.Dados) : NotFound(result.Mensagem);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] TurmaDtoCreate dto, [FromServices] CriarTurmaUseCase useCase)
    {
        var result = await useCase.ExecutarAsync(dto);
        return result.Sucesso
            ? CreatedAtAction(nameof(ObterPorId), new { id = result.Dados }, result)
            : BadRequest(result.Mensagem);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] TurmaDtoUpdate dto, [FromServices] AtualizarTurmaUseCase useCase)
    {
        var result = await useCase.ExecutarAsync(id, dto);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Mensagem);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deletar(Guid id, [FromServices] RemoverTurmaUseCase useCase)
    {
        var result = await useCase.ExecutarAsync(id);
        return result.Sucesso ? NoContent() : BadRequest(result.Mensagem);
    }
}