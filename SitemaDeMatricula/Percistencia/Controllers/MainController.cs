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
        return result.Tipo switch
        {
            TipoErro.Conflito => Conflict(new { mensagem = result.Mensagem }), // Retorna 409
            TipoErro.NaoEncontrado => NotFound(new { mensagem = result.Mensagem }), // Retorna 404
            _ => BadRequest(new { mensagem = result.Mensagem }) // Retorna 400 por padrão
        };
    }
}