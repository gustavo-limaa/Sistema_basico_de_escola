using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Aplicacao.Dtos.Matricola;
using SitemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SitemaDeMatricula.Percistencia.Controllers;

[Route("api/matriculas")]
public class MatriculaController : MainController
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