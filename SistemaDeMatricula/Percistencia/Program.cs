using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SistemaDeMatricula.Infraestrutura;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Percistencia.Controllers;
using SistemaDeMatricula.Percistencia.Middleware;

Env.Load("../.env");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(ProfessorController).Assembly)
    .AddApplicationPart(typeof(DisciplinaController).Assembly);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSecurityConfiguration(builder.Configuration);

var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));
builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing")) // Ajuste conforme o nome do seu ambiente
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sistema de Matrícula - API")
               .WithTheme(ScalarTheme.Mars)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.MapGet("/api/teste", () => "O servidor está ouvindo!").WithName("Teste");
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{ }