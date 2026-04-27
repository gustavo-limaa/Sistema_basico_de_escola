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

        if (!resultado.Sucesso)
        {
            return resultado.Mensagem.Contains("não encontrada")
                ? NotFound(resultado.Mensagem)
                : BadRequest(resultado.Mensagem);
        }
        return Ok(resultado.Dados);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id, [FromServices] RemoverUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);

        if (resultado.Sucesso) return NoContent();

        // Se a mensagem for de "não encontrada", mandamos 404. Se for outra coisa, 400.
        return resultado.Mensagem.Contains("não encontrada")
            ? NotFound(resultado.Mensagem)
            : BadRequest(resultado.Mensagem);
    }

    [HttpPatch("{id}/restaurar")]
    public async Task<IActionResult> Restaurar(Guid id, [FromServices] RestaurarUseCaseDisciplina useCase)
    {
        var resultado = await useCase.Executar(id);

        // Se deu certo, retorna 200 OK com os dados. Se não, 404.
        // 1. Caso de Sucesso
        if (resultado.Sucesso)
            return Ok(resultado.Dados);

        // 2. Caso de Conflito (Já está ativa)
        if (resultado.Mensagem.Contains("Esta disciplina já está ativa e não precisa ser restaurada."))
            return Conflict(resultado.Mensagem);

        // 3. Caso de Não Encontrado (ID inexistente ou não desativado)
        if (resultado.Mensagem.Contains("Disciplina desativada não encontrada."))
            return NotFound(resultado.Mensagem);

        // 4. Caso genérico de erro (Fallback)
        return BadRequest(resultado.Mensagem);
    }
}