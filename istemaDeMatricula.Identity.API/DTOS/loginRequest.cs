namespace SistemaDeMatricula.Identity.API.DTOS;

public sealed record loginRequest(
    string? Email,
    string? Password
);