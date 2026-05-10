using SistemaDeMatricula.Domain;
using SistemaDeMatricula.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Aplicacao.Usecases.Estudante;

public sealed class UsecaseVerificarCpfEstudante
{
    private readonly IRepositorioEstudante _repositorio;

    public UsecaseVerificarCpfEstudante(IRepositorioEstudante repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Result<bool>> Executar(string cpf)
    {
        var cpfLimpo = cpf.Replace(".", "").Replace("-", "");

        var existe = await _repositorio.ExisteCpfAsync(cpfLimpo);

        if (!existe)
        {
            return Result<bool>.Falha("Estudante não encontrado.");
        }

        return Result<bool>.Ok(true, "Estudante Localizado.");
    }
};