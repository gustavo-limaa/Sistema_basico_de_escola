using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Aplicacao.Usecases.Turmas;

namespace SitemaDeMatricula.Percistencia.Controllers;

[Route("api/[controller]")]
public class TurmasController : MainController
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromServices] ListarTurmaUsecase useCase)
        => CustomResponse(await useCase.ExecutarAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, [FromServices] ObterPorIdTurma useCase)
        => CustomResponse(await useCase.ExecutarAsync(id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] TurmaDtoCreate dto, [FromServices] CriarTurmaUseCase useCase)
    {
        var result = await useCase.ExecutarAsync(dto);

        if (result.Sucesso)
            return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados.Id }, result.Dados);

        return CustomResponse(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] TurmaDtoUpdate dto, [FromServices] AtualizarTurmaUseCase useCase)
        => CustomResponse(await useCase.ExecutarAsync(id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deletar(Guid id, [FromServices] RemoverTurmaUseCase useCase)
        => CustomResponse(await useCase.ExecutarAsync(id));
}