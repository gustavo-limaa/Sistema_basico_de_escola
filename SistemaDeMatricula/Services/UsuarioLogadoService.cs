using System.Security.Claims;

namespace SistemaDeMatricula.Services;

public class UsuarioLogadoService : IUsuarioLogadoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioLogadoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string ObterUsuarioId()
    {
        var estaAutenticado = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        if (!estaAutenticado)
        {
            throw new UnauthorizedAccessException("Usuário não está autenticado no sistema.");
        }

        var claimUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        if (claimUser == null)
        {
            throw new Exception("Usuário logado, mas o Token não possui a propriedade de ID (NameIdentifier).");
        }

        return claimUser.Value;
    }
}