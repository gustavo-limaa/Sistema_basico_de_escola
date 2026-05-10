namespace SistemaDeMatricula.Domain.Value_Object;

public record CargaHoraria
{
    public int Valor { get; private init; }

    public CargaHoraria(int valor)
    {
        // Regra de negócio: Mínimo 1h, Máximo 200h (exemplo)
        if (valor <= 0)
            throw new ArgumentException("A carga horária deve ser maior que zero.");

        if (valor > 200)
            throw new ArgumentException("Carga horária excessiva! O máximo permitido é 200h.");

        Valor = valor;
    }

    private CargaHoraria() { } // Para EF Core

    public static implicit operator int(CargaHoraria ch) => ch.Valor;
    public static implicit operator CargaHoraria(int valor) => new CargaHoraria(valor);
}