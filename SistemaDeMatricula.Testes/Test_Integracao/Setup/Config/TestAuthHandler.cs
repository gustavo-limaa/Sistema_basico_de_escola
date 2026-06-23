using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

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
            // 🎯 AQUI ESTÁ A MÁGICA DA SUA ROLE MASTER!
            // Criamos as claims fictícias que vão enganar as tags [Authorize] nos testes de integração
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, "UsuarioTeste"),
            new Claim(ClaimTypes.Role, "Admin") // 👑 Dá poder total (Admin) para os 134 testes passarem direto
        };

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            // Retorna o crachá com sucesso para o ciclo de vida do .NET de teste
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}