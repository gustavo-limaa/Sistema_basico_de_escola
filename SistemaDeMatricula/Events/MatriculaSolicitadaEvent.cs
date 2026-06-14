namespace SistemaDeMatricula.Events;

public record MatriculaSolicitadaEvent
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public Guid AlunoId { get; init; }
    public Guid TurmaId { get; init; }

    public string UsuarioId { get; init; } = string.Empty;
    public string Origem { get; init; } = "SistemaDeMatricula.API";

    public DateTime OcorridoEm { get; init; } = DateTime.UtcNow;
}