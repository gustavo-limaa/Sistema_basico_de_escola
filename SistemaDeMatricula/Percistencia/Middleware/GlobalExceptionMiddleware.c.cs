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
        // 🎯 1. Captura ou gera um CorrelationId único para rastrear a requisição
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        // 🎯 2. Garante a adição do header de resposta de forma segura usando OnStarting
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("X-Correlation-ID"))
            {
                context.Response.Headers.Append("X-Correlation-ID", correlationId);
            }
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // 🎯 3. Log Estruturado
            _logger.LogError(
                ex,
                "Exceção capturada no Middleware [CorrelationId: {CorrelationId}] | Rota: {Method} {Path}",
                correlationId,
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            DomainException => (int)HttpStatusCode.BadRequest,      // 400
            ArgumentException => (int)HttpStatusCode.BadRequest,    // 400
            KeyNotFoundException => (int)HttpStatusCode.NotFound,   // 404
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden, // 403
            _ => (int)HttpStatusCode.InternalServerError            // 500
        };

        var response = new
        {
            status = context.Response.StatusCode,
            mensagem = exception.Message,
            correlationId = correlationId, // 🎯 Adiciona o ID de rastreamento no payload do JSON
            detalhes = context.Response.StatusCode == (int)HttpStatusCode.InternalServerError
                ? "Ocorreu um erro interno no servidor. Contate o suporte com o CorrelationId fornecido."
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