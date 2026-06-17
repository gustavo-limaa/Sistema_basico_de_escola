using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SistemaDeMatricula.Percistencia.Middleware;

public static class SecurityConfiguration
{
    public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var chaveSecreta = configuration["JWT_KEY"]
            ?? throw new InvalidOperationException("A chave secreta do JWT (JWT_KEY) não foi configurada!");

        var emisor = configuration["JWT_ISSUER"] ?? "SistemaMatriculaAPI";
        var audiencia = configuration["JWT_AUDIENCE"] ?? "SistemaMatriculaAPI";

        var chaveEmBytes = Encoding.ASCII.GetBytes(chaveSecreta);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(chaveEmBytes),

                ValidateIssuer = true,
                ValidIssuer = emisor,

                ValidateAudience = true,
                ValidAudience = audiencia,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}