using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;

namespace SistemaDeMatricula.Domain.Modelos
{
    public abstract class ModeloMain
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public bool Ativo { get; protected set; } = true;

        protected ModeloMain()
        {
            Id = Guid.NewGuid();

            Ativo = true;
        }

        public void ativar() => Ativo = true;

        public void desativar() => Ativo = false;
    }
}