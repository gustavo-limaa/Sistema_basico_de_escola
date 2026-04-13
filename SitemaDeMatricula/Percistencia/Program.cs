using Microsoft.EntityFrameworkCore;
using SitemaDeMatricula.Aplicacao;
using SitemaDeMatricula.InfraEstrutura.Data;

var builder = WebApplication.CreateBuilder(args);

// --- TUDO QUE É CONFIGURAÇÃO DE SERVIÇO FICA AQUI (ANTES DO BUILD) ---

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication(); // Método de extensão para registrar os Use Cases (Dependency Injection)

// 1. Recupera a Connection String do User Secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Configura o DbContext para usar o MySQL (Pomelo)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

var app = builder.Build();

// --- DAQUI PARA BAIXO É O PIPELINE DE EXECUÇÃO ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();