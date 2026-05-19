using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Percistencia.Controllers;

[Route("api/matriculas")]
public sealed class MatriculaController : MainController
{
    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] MatriculaDtoCreate dtoCreate,
        [FromServices] MatricularEstudanteUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync(dtoCreate));

    [HttpGet]
    public async Task<IActionResult> PegarTodos([FromServices] ListarTodasMatriculasUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> PegarPorId(Guid id, [FromServices] ObterMatriculaPorIdUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync(id));

    [HttpPatch("{id:guid}/transferir")]
    public async Task<IActionResult> Transferir(
        Guid id,
        [FromBody] Guid novaTurmaId,
        [FromServices] TransferirEstudanteUsecase usecase)
    {
        return CustomResponse(await usecase.ExecutarAsync(id, novaTurmaId));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancelar(Guid id, [FromServices] DesativarMatriculaUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync(id));
}