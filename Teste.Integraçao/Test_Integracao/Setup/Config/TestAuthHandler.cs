using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SistemaDeMatricula.Testes.Test_Integracao.Setup.Config
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Criamos uma lista base de claims com o ID que já corrigimos
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "UsuarioTeste"),
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
    };

            // 2. Olhamos se a requisição de teste enviou o cabeçalho mandando usar uma Role específica
            if (Context.Request.Headers.TryGetValue("X-Test-Role", out var roleCustomizada))
            {
                // Se enviou (ex: "Estudante"), adicionamos APENAS essa role no crachá!
                claims.Add(new Claim(ClaimTypes.Role, roleCustomizada.ToString()));
            }
            else
            {
                // Se NÃO enviou o cabeçalho, mantém o comportamento antigo (Superpoderes)
                // para não quebrar nenhum dos seus 271 testes antigos!
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim(ClaimTypes.Role, "Estudante"));
                claims.Add(new Claim(ClaimTypes.Role, "Professor"));
            }

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}