using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SistemaDeMatricula.Identity.API.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaDeMatricula.Identity.API.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(IdentityUser user, IList<string> roles)
        {
            var chaveSecreta = _configuration["JWT_KEY"]
                 ?? throw new InvalidOperationException("A chave secreta JWT_KEY não foi configurada!");
            var emisor = _configuration["JWT_ISSUER"]
                 ?? throw new InvalidOperationException("O emissor JWT_ISSUER não foi configurado!");

            var audiencia = _configuration["JWT_AUDIENCE"]
                 ?? throw new InvalidOperationException("A audiência JWT_AUDIENCE não foi configurada!");
            var claims = new List<System.Security.Claims.Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new  Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: emisor,
                audience: audiencia,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}