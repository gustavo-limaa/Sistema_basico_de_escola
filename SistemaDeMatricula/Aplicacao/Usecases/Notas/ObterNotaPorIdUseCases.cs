using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas
{
    public class ObterNotaPorIdUseCases
    {
        private readonly IUnitOfWork _uow;

        public ObterNotaPorIdUseCases(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, Guid notaId)
        {
            var matricula = await _uow.Matriculas.ObterPorIdAsync(matriculaId);
            if (matricula == null)
                return Result<NotaDtoResponse>.NaoEncontrado("Matrícula não encontrada.");

            var nota = matricula.Notas.FirstOrDefault(n => n.Id == notaId);
            if (nota == null)
                return Result<NotaDtoResponse>.NaoEncontrado("Nota não encontrada.");
            var notaDtoResponse = nota.ToNotaDtoResponse();
            return Result<NotaDtoResponse>.Ok(notaDtoResponse);
        }
    }
}