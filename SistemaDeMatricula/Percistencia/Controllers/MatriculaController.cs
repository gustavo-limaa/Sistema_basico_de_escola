using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Aplicacao.Usecases.Notas;
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

    // endpoint para notas
    [HttpPost("{id:guid}/notas")]
    public async Task<IActionResult> AdicionarNota(Guid id, [FromBody] NotaDtoCreate notaDtoCreate,
        [FromServices] AdicionarNotasMatriculaUseCase usecase)
    {
        return CustomResponse(await usecase.ExecuteAsAsync(id, notaDtoCreate), isCreated: true);
    }

    [HttpGet("{id:guid}/notas")]
    public async Task<IActionResult> PegarNotas([FromServices] ListarTodasAsNotasUsecase usecase) =>
        CustomResponse(await usecase.ExecuteAsAsync());

    [HttpGet("{id:guid}/notas/{notaId:guid}")]
    public async Task<IActionResult> PegarNotaPorId(Guid notaId, [FromServices] ObterNotaPorIdUseCases usecase) =>
        CustomResponse(await usecase.ExecuteAsAsync(notaId));

    [HttpPut("{id:guid}/notas/{notaId:guid}")]
    public async Task<IActionResult> AtualizarNota(Guid notaId, [FromBody] NotaDtoUpdate notaDtoUpdate, [FromServices] AtualizarNotaUsecase usecase) =>
        CustomResponse(await usecase.ExecuteAsAsync(notaId, notaDtoUpdate));
}