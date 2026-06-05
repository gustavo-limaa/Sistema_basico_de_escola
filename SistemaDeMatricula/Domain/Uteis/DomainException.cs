namespace SistemaDeMatricula.Domain.Uteis
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}