using SistemaDeMatricula.Domain.Uteis;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SistemaDeMatricula.Percistencia.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro não tratado.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            DomainException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,    // Erro de validação dos seus VOs
            KeyNotFoundException => (int)HttpStatusCode.NotFound,   // Objeto não encontrado
            _ => (int)HttpStatusCode.InternalServerError            // Erro genérico (500)
        };

        var response = new
        {
            status = context.Response.StatusCode,
            mensagem = exception.Message, // Aqui vai a mensagem que você escreveu no VO!
            detalhes = context.Response.StatusCode == 500 ? "Erro interno no servidor." : null
        };
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}