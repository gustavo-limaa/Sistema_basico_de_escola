using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SistemaDeMatricula.Infraestrutura;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Percistencia.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(ProfessorController).Assembly)
    //.AddApplicationPart(typeof(EstudanteController).Assembly)
    .AddApplicationPart(typeof(DisciplinaController).Assembly);
builder.Services.AddOpenApi();
builder.Services.AddApplication();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{ }