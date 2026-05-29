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

            if (nota is null)
                return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada.");

            // 3. Validação de dono (Só acessa o MatriculaId SE a nota não for nula)
            if (nota.MatriculaId != matriculaId)
                return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada para esta matrícula.");
            return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
        }
    }
}