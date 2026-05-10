namespace SistemaDeMatricula.Domain.Modelos
{
    public abstract class ModeloMain
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public bool Ativo { get; set; } = true;
    }
}