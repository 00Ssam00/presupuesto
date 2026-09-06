using System.Text.Json;
using Quincena.Api.Serialization;

namespace Quincena.Api.Middleware;

// Port de backend/src/middleware/errorHandler.ts. En producción no se expone
// ex.Message crudo (puede filtrar detalles de Postgres, como nombres de
// tablas/columnas) — solo un mensaje genérico. El detalle completo sigue
// disponible en los logs de arriba.
public class ExceptionHandlingMiddleware
{
    private const string GenericMessage = "Error interno del servidor";

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment environment, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _environment = environment;
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
            _logger.LogError(ex, "Unhandled exception");
            var message = _environment.IsProduction()
                ? GenericMessage
                : (string.IsNullOrEmpty(ex.Message) ? GenericMessage : ex.Message);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }, JsonOptions));
        }
    }
}
