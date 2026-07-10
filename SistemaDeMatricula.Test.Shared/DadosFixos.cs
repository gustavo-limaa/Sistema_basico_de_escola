using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Test.Shared;

public class DadosFixos
{
    public const string ErroInternoServidor = "Ocorreu um erro interno no servidor. Contate o administrador.";
    public const string ErroNaoEncontrado = "Registro não encontrado.";
    public const string ErroNaoAutorizado = "Não autorizado.";
    public const string ErroConflito = "Conflito de dados.";
    public const string ErroValidacao = "Erro de validação.";
    public const string ErroInesperado = "Erro inesperado.";
    public const string ErroRequisicaoInvalida = "Requisição inválida.";
    public const string ErroRequisicaoInvalidaDetalhes = "A requisição não pôde ser processada devido a erros de validação.";
}