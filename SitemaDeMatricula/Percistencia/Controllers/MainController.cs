using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Domain;

namespace SistemaDeMatricula.Percistencia.Controllers;

[ApiController]
public abstract class MainController : ControllerBase
{
    protected ActionResult CustomResponse<T>(Result<T> result)
    {
        if (result.Sucesso)
        {
            if (result.Dados == null) return NoContent();

            return Ok(result.Dados);
        }
        return result.Tipo switch
        {
            TipoErro.Conflito => Conflict(new { mensagem = result.Mensagem }),
            TipoErro.NaoEncontrado => NotFound(new { mensagem = result.Mensagem }),
            _ => BadRequest(new { mensagem = result.Mensagem })
        };
    }
}