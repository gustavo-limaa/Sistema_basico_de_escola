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

        if (!resultado.Sucesso)
            return BadRequest(resultado.Mensagem);

        // ✅ Retornamos apenas o DTO (resultado.Dados)
        // para o JSON ficar no formato que o teste espera
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado!.Dados!.DisciplinaId }, resultado!.Dados);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id, [FromServices] ObterPorIdUsecaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);
        return resultado.Sucesso ? Ok(resultado.Dados) : NotFound(resultado.Mensagem);
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodas([FromServices] ObterTodasDisciplinaUseCase useCase)
    {
        var resultado = await useCase.Executar();

        // 1. Se o UseCase falhou (erro de banco, etc), mandamos 400.
        if (!resultado.Sucesso)
            return BadRequest(resultado.Mensagem);

        // 2. Se deu sucesso, a Controller checa o conteúdo:
        // Tem itens? 200 OK. Tá vazia? 204 No Content.
        return resultado.Dados.Any()
            ? Ok(resultado.Dados)
            : NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] DisciplinaDtoUpdate dto, [FromServices] AtualizarUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id, dto);
        return resultado.Sucesso ? Ok(resultado.Dados) : BadRequest(resultado.Mensagem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id, [FromServices] RemoverUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Mensagem);
    }
}