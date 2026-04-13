using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.InfraEstrutura.Data;

namespace SitemaDeMatricula.Infraestrutura.Repositorios;

public class RepositorioEstudante : IRepositorioEstudante
{
    private readonly AppDbContext _Context;

    public RepositorioEstudante(AppDbContext context)
    {
        _Context = context;
    }

    public async Task<Result<Estudante>> AdicionarAsync(Estudante estudante)
    {
        try
        {
            if (estudante == null) return Result<Estudante>.Falha("Estudante não pode ser nulo");

            if (estudante.Cpf == null) return Result<Estudante>.Falha("CPF é obrigatório");

            if (await _Context.Estudantes.AnyAsync(e => e.Cpf.Valor == estudante.Cpf.Valor)) return Result<Estudante>.Falha("CPF já cadastrado para outro estudante");

            await _Context.Estudantes.AddAsync(estudante);

            var salvar = await _Context.SaveChangesAsync();

            if (salvar == 0) return Result<Estudante>.Falha("Erro ao adicionar estudante");
            return Result<Estudante>.Ok(estudante);
        }
        catch (Exception ex)
        {
            // Aqui você captura o erro, mas não deixa o app travar
            // Você pode tratar erros específicos, como CPF duplicado
            return Result<Estudante>.Falha($"Erro interno ao salvar: {ex.Message}");
        }
    }

    public async Task<Result<Estudante>> AtualizarAsync(Estudante estudante)
    {
        try
        {
            if (estudante == null) return Result<Estudante>.Falha("Estudante não pode ser nulo");

            _Context.Estudantes.Update(estudante);

            var salvar = await _Context.SaveChangesAsync();

            if (salvar == 0) return Result<Estudante>.Falha("Erro ao atualizar estudante");

            return Result<Estudante>.Ok(estudante);
        }
        catch (Exception ex)
        {
            return Result<Estudante>.Falha($"Erro interno ao atualizar: {ex.Message}");
        }
    }

    public async Task<Result<Estudante>> ObterPorIdAsync(Guid estudanteId)
    {
        try
        {
            if (estudanteId == Guid.Empty) return Result<Estudante>.Falha("ID do estudante é inválido");

            var estudante = await _Context.Estudantes
                .Include(e => e.Matriculas) // Inclui as matrículas relacionadas
                .FirstOrDefaultAsync(e => e.EstudanteId == estudanteId);

            if (estudante == null) return Result<Estudante>.Falha("Estudante não encontrado");

            return Result<Estudante>.Ok(estudante);
        }
        catch (Exception ex)
        {
            return Result<Estudante>.Falha($"Erro interno ao obter estudante: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Estudante>>> ObterTodosAsync()
    {
        try
        {
            var estudantes = await _Context.Estudantes
                .AsNoTracking() // Dica de performance!
                .Include(e => e.Matriculas)
                .ToListAsync();

            // Remova os IFs de erro para lista vazia.
            // Se a lista vier vazia, retornamos OK com a lista vazia.
            return Result<IEnumerable<Estudante>>.Ok(estudantes);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Estudante>>.Falha($"Erro interno: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoverAsync(Guid estudanteId)
    {
        try
        {
            if (estudanteId == Guid.Empty) return Result<bool>.Falha("ID do estudante é inválido");
            var estudante = await _Context.Estudantes.FindAsync(estudanteId);
            if (estudante == null) return Result<bool>.Falha("Estudante não encontrado");
            _Context.Estudantes.Remove(estudante);
            var salvar = await _Context.SaveChangesAsync();
            if (salvar == 0) return Result<bool>.Falha("Erro ao remover estudante");
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Falha($"Erro interno ao remover estudante: {ex.Message}");
        }
    }
}