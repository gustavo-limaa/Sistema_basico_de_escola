using Microsoft.EntityFrameworkCore;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas
{
    public class ObterNotasPorMatricula
    {
        private readonly IUnitOfWork _uow;

        public ObterNotasPorMatricula(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<IEnumerable<NotaDtoResponse>>> ExecuteAsAsync(Guid matriculaId)
        {
            // Agora o banco filtra lá no servidor!
            var notas = await _uow.Notas.ObterNotasporMatricula(matriculaId).ToListAsync();

            return Result<IEnumerable<NotaDtoResponse>>.Ok(notas.Select(n => n.ToNotaDtoResponse()));
        }
    }
}