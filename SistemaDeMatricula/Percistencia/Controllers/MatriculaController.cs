using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Aplicacao.Usecases.Notas;
using SistemaDeMatricula.Percistencia.Controllers;

[Authorize]
[Route("api/matriculas")]
public sealed class MatriculaController : MainController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar(
        [FromBody] MatriculaDtoCreate dtoCreate,
        [FromServices] MatricularEstudanteUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync(dtoCreate));

    [HttpGet]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> PegarTodos([FromServices] ListarTodasMatriculasUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync());

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> PegarPorId(Guid id, [FromServices] ObterMatriculaPorIdUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync(id));

    [HttpPatch("{id:guid}/transferir")]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> Transferir(
        Guid id,
        [FromBody] Guid novaTurmaId,
        [FromServices] TransferirEstudanteUsecase usecase)
    {
        return CustomResponse(await usecase.ExecutarAsync(id, novaTurmaId));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancelar(Guid id, [FromServices] DesativarMatriculaUsecase usecase) =>
        CustomResponse(await usecase.ExecutarAsync(id));

    // endpoint para notas
    [HttpPost("{id:guid}/notas")]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> AdicionarNota(Guid id, [FromBody] NotaDtoCreate notaDtoCreate,
        [FromServices] AdicionarNotasMatriculaUseCase usecase)
    {
        return CustomResponse(await usecase.ExecuteAsAsync(id, notaDtoCreate), isCreated: true);
    }

    [HttpGet("{id:guid}/notas")]
    [Authorize(Roles = "Admin,Professor,Estudante")]
    public async Task<IActionResult> PegarNotas(Guid id, [FromServices] ListarTodasAsNotasUsecase usecase) =>
        CustomResponse(await usecase.ExecuteAsAsync(id));

    [HttpGet("{id:guid}/notas/{notaId:guid}")]
    [Authorize(Roles = "Admin,Professor,Estudante")]
    public async Task<IActionResult> PegarNotaPorId(Guid id, Guid notaId, [FromServices] ObterNotaPorIdUseCases usecase) =>
        CustomResponse(await usecase.ExecuteAsAsync(id, notaId));

    [HttpPut("{id:guid}/notas/{notaId:guid}")]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> AtualizarNota(Guid id, Guid notaId, [FromBody] NotaDtoUpdate notaDtoUpdate, [FromServices] AtualizarNotaUsecase usecase) =>
        CustomResponse(await usecase.ExecuteAsAsync(id, notaId, notaDtoUpdate));

    // Exemplo temporário de debug no controller
    [HttpGet("{matriculaId}/notas")]
    [Authorize(Roles = "Admin,Professor")]
    public async Task<IActionResult> ListarNotas(Guid matriculaId, [FromServices] ObterNotasPorMatricula useCase)
    {
        var result = await useCase.ExecuteAsAsync(matriculaId);
        return CustomResponse(result);
    }
}