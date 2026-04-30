using Microsoft.AspNetCore.Mvc;
using SitemaDeMatricula.Domain;

namespace SitemaDeMatricula.Percistencia.Controllers;

[ApiController]
public abstract class MainController : ControllerBase
{
    protected ActionResult CustomResponse<T>(Result<T> result)
    {
        if (result.Sucesso)
        {
            // Se não houver dados (ex: Delete), retornamos 204 No Content
            if (result.Dados == null) return NoContent();

            return Ok(result.Dados);
        }
        // Switch para decidir o status code baseado no tipo do erro
        return result.Tipo switch
        {
            TipoErro.NaoEncontrado => NotFound(new { mensagem = result.Mensagem }),
            TipoErro.Conflito => Conflict(new { mensagem = result.Mensagem }), // 409 Aqui!
            _ => BadRequest(new { mensagem = result.Mensagem }) // 400 para o resto
        };
        // Se falhou, retornamos 400 com a sua mensagem
        return BadRequest(new { mensagem = result.Mensagem });
    }
}