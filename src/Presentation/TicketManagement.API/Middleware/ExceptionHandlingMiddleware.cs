using System.Net;
using System.Text.Json;
using FluentValidation;
using TicketManagement.Application.Common.Exceptions;

namespace TicketManagement.WebApi.Middleware;

/// <summary>
/// Middleware global para capturar excepciones y convertirlas en respuestas HTTP apropiadas
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        object? errors;

        switch (exception)
        {
            case Application.Common.Exceptions.ValidationException validationEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Validation failed";
                errors = validationEx.Errors;
                break;

            case NotFoundException notFoundEx:
                statusCode = (int)HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                errors = null;
                break;

            case ForbiddenAccessException forbiddenEx:
                statusCode = (int)HttpStatusCode.Forbidden;
                message = forbiddenEx.Message;
                errors = null;
                break;

            case Domain.Exceptions.DomainException domainEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = domainEx.Message;
                errors = null;
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An internal server error occurred. ";
                errors = null;
                break;
        }

        var response = new
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors
        };

        context.Response.StatusCode = statusCode;

        // Log según severidad
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Internal Server Error:  {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Client Error: {Message}", exception.Message);
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
