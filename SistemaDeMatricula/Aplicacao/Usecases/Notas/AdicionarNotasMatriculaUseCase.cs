using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Mapper;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Infraestrutura.Repositorios;
using System.Security.AccessControl;

namespace SistemaDeMatricula.Aplicacao.Usecases.Notas;

public sealed class AdicionarNotasMatriculaUseCase
{
    private readonly IUnitOfWork _uow;

    public AdicionarNotasMatriculaUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<NotaDtoResponse>> ExecuteAsAsync(Guid matriculaId, NotaDtoCreate notaDtoCreate)
    {
        var matricula = await _uow.Matriculas.ObterPorIdAsync(matriculaId);
        if (matricula == null)
            return Result<NotaDtoResponse>.NaoEncontrado("Matrícula não encontrada.");

        if (!matricula.Ativo)
            return Result<NotaDtoResponse>.Falha("Não é possível adicionar notas a uma matrícula inativa.");

        // Cria a nota (Domínio)
        var nota = new Nota
       (

        valor: notaDtoCreate.Valor,
        descricao: notaDtoCreate.Descricao,
        importancia: notaDtoCreate.Importancia,
        categoria: notaDtoCreate.Categoria,
        dataEmissao: DateTime.UtcNow,
        matriculaId: matriculaId
       );

        // Adiciona de forma explícita via repositório
        await _uow.Notas.AdicionarAsync(nota);

        // Commit via UnitOfWork
        var sucesso = await _uow.CommitAsync();
        if (!sucesso)
            return Result<NotaDtoResponse>.Falha("Erro ao salvar nota no banco.");

        return Result<NotaDtoResponse>.Ok(nota.ToNotaDtoResponse());
    }
}