using Microsoft.AspNetCore.Identity;

namespace SistemaDeMatricula.Identity.API.Interface
{
    public interface ITokenService
    {
        string GenerateToken(IdentityUser user, IList<string> roles);
    }
}