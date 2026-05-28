using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
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

            if (nota.MatriculaId != matriculaId)
                return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada para a matrícula informada.");
            if (nota.Id != notaId)
                return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada para a matrícula informada.");

            if (nota is null)
                return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada para a matrícula informada.");
            return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
        }
    }
}