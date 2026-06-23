namespace SistemaDeMatricula.Identity.API.DTOS
{
    public record LoginResponse(string Email, string Token, IList<string> Roles);
}