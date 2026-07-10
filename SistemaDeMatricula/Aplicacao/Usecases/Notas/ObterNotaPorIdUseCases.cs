using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Erros;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas
{
    public sealed class ObterNotaPorIdUseCases
    {
        private readonly IUnitOfWork _uow;

        public ObterNotaPorIdUseCases(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, Guid notaId)
        {
            var nota = await _uow.Notas.ObterPorId(notaId);

            if (nota is null)
                return Result<NotaDtoResponse>.NaoEncontrado(MensagensNotas.NotaNaoEncontrada);

            if (nota.MatriculaId != matriculaId)
                return Result<NotaDtoResponse>.NaoEncontrado(MensagensNotas.NotaNaoEncontrada);
            return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
        }
    }
}