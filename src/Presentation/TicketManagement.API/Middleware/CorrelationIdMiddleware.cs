using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TicketManagement.WebApi.Middleware;

/// <summary>
/// ? Middleware para agregar Correlation ID a todas las requests
/// Permite trazabilidad completa en logs y debugging
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener o generar Correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Agregar a HttpContext.Items para acceso en toda la pipeline
        context.Items["CorrelationId"] = correlationId;

        // Agregar a response headers
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        // Agregar a logs (ILogger lo capturará automáticamente)
        using (context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>()
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Extension method para registrar el middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
