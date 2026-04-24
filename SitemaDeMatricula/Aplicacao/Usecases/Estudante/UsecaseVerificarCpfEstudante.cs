using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitemaDeMatricula.Aplicacao.Usecases.Estudante;

public class UsecaseVerificarCpfEstudante
{
    private readonly IRepositorioEstudante _repositorio;

    public UsecaseVerificarCpfEstudante(IRepositorioEstudante repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Result<bool>> Executar(string cpf)
    {
        // 1. Limpeza (Sempre bom!)
        var cpfLimpo = cpf.Replace(".", "").Replace("-", "");

        // 2. Pergunta ao banco se existe
        var existe = await _repositorio.ExisteCpfAsync(cpfLimpo);

        // 3. Se NÃO existe (false), retorna falha
        if (!existe)
        {
            return Result<bool>.Falha("Estudante não encontrado.");
        }

        // 4. Se existe (true), retorna sucesso
        return Result<bool>.Ok(true, "Estudante Localizado.");
    }
};