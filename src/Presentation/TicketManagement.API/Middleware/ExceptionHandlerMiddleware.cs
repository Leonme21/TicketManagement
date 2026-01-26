using System.Net;
using System.Text.Json;
using TicketManagement.Application.Common.Exceptions;

namespace TicketManagement.API.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var problemDetails = new
        {
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = exception.Message
        };

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                // Usamos un formato más estructurado para errores de validación
                return context.Response.WriteAsJsonAsync(new { 
                    Title = "Validation Error", 
                    Status = (int)HttpStatusCode.BadRequest,
                    Errors = validationException.Errors 
                });

            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails = problemDetails with { Title = "Resource not found." };
                break;

            // Puedes añadir más casos para excepciones personalizadas
            // case UnauthorizedAccessException:
            //    response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var result = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(result);
    }
}
