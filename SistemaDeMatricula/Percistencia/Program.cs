using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SistemaDeMatricula.Infraestrutura;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Percistencia.Controllers;
using SistemaDeMatricula.Percistencia.Middleware;
using System.Text.Json;

Env.Load("../.env");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(ProfessorController).Assembly)
    .AddApplicationPart(typeof(DisciplinaController).Assembly);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSecurityConfiguration(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

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

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                componente = e.Key,
                status = e.Value.Status.ToString(),
                descricao = e.Value.Description,
                duracao = e.Value.Duration.TotalMilliseconds + " ms"
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
});
app.MapControllers();

app.Run();

public partial class Program
{ }