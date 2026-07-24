namespace SistemaDeMatricula.Services
{
    public interface IUsuarioLogadoService
    {
        public string ObterUsuarioId();

        public bool Ehadmin();
    }
}