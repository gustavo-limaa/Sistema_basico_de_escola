using SistemaDeMatricula.Testes.Test_Integracao.Setup;
using System.Net.Http.Headers;

namespace SistemaDeMatricula.Testes.Test_Integracao;

public abstract class PermissaoTestBase : IClassFixture<SistemaMatriculaFactory>
{
    protected readonly SistemaMatriculaFactory _Factory;
    protected readonly HttpClient _Client;

    protected PermissaoTestBase(SistemaMatriculaFactory factory)
    {
        _Factory = factory;
        _Client = factory.CreateClient();
    }

    protected void AutenticarComoEstudante()
    {
        _Client.DefaultRequestHeaders.Remove("X-Test-Role");
        _Client.DefaultRequestHeaders.Add("X-Test-Role", "Estudante");
    }

    protected void AutenticarComoProfessor()
    {
        _Client.DefaultRequestHeaders.Remove("X-Test-Role");
        _Client.DefaultRequestHeaders.Add("X-Test-Role", "Professor");
    }

    protected void RemoverAutenticacao()
    {
        _Client.DefaultRequestHeaders.Authorization = null;
        _Client.DefaultRequestHeaders.Remove("X-Test-Role");
    }

    protected void ResetarParaAdmin()
    {
        _Client.DefaultRequestHeaders.Remove("X-Test-Role");
        _Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }
}