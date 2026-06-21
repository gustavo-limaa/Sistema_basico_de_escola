namespace SistemaDeMatricula.Identity.API.DTOS;

public sealed record RegistroRequest(
    string Email,
    string Password,
    string Role
);