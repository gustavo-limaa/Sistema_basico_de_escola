using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Aplicacao;
using SitemaDeMatricula.InfraEstrutura.Data;
using Scalar.AspNetCore;
using SitemaDeMatricula.Percistencia.Controllers;

// <-- Adicione esse using!

var builder = WebApplication.CreateBuilder(args);

// --- TUDO QUE É CONFIGURAÇÃO DE SERVIÇO FICA AQUI (ANTES DO BUILD) ---
// No Program.cs, procure a linha do AddControllers e mude para isso:
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

        // Se o WithEndpoint der erro, não use ele.
        // Por padrão o Scalar já tenta ler o /openapi/v1.json
    });
}

app.MapGet("/api/teste", () => "O servidor está ouvindo!").WithName("Teste");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();