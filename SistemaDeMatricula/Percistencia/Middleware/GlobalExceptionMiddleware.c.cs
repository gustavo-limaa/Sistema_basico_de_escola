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
            DomainException => (int)HttpStatusCode.BadRequest,      // 400 Regra de negócio violada
            ArgumentException => (int)HttpStatusCode.BadRequest,    // 400 Erro nos seus VOs / Tipos
            KeyNotFoundException => (int)HttpStatusCode.NotFound,   // 404 Objeto sumiu
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden, // 403 Se estourar algo de token
            _ => (int)HttpStatusCode.InternalServerError            // 500 Erro bruto (banco caiu, etc)
        };

        var response = new
        {
            status = context.Response.StatusCode,
            mensagem = exception.Message,
            // Só expõe detalhes internos se for um erro 500 bruto
            detalhes = context.Response.StatusCode == (int)HttpStatusCode.InternalServerError
                ? "Ocorreu um erro interno no servidor. Contate o administrador."
                : null
        };

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}