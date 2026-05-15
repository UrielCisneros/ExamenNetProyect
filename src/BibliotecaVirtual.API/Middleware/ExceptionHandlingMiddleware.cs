using System.Net;
using System.Text.Json;
using BibliotecaVirtual.Domain.Exceptions;

namespace BibliotecaVirtual.API.Middleware;

/// <summary>
/// Middleware global que captura excepciones y devuelve un payload JSON estándar.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        HttpStatusCode status = ex switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            ConflictException => HttpStatusCode.Conflict,
            ForbiddenException => HttpStatusCode.Forbidden,
            ValidationException => HttpStatusCode.BadRequest,
            DomainException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Excepción no controlada");
        else
            _logger.LogWarning(ex, "Excepción controlada de dominio");

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)status;

        object payload = ex is ValidationException vex
            ? new { error = vex.Message, errores = vex.Errores }
            : new { error = ex.Message };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
