namespace SistemaDeMatricula.Identity.API.DTOS
{
    public class loginRequest
    {
        public string? Email { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
    }
}