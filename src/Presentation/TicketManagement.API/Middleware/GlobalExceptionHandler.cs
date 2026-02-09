using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.API.Middleware;

/// <summary>
/// 🔥 PRODUCTION-READY: Global exception handler with structured error responses
/// Features:
/// - Structured error responses with correlation IDs
/// - Security-aware error messages (no sensitive data exposure)
/// - Proper HTTP status code mapping
/// - Comprehensive logging with context
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;
        
        // Log with structured context
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = httpContext.Request.Path,
            ["RequestMethod"] = httpContext.Request.Method,
            ["UserId"] = GetUserId(httpContext),
            ["UserAgent"] = httpContext.Request.Headers.UserAgent.ToString(),
            ["RemoteIpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        });

        var (statusCode, title, detail, errorCode) = MapException(exception);

        _logger.LogError(exception, 
            "Unhandled exception occurred. Status: {StatusCode}, ErrorCode: {ErrorCode}", 
            statusCode, errorCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["correlationId"] = correlationId,
                ["errorCode"] = errorCode,
                ["timestamp"] = DateTimeOffset.UtcNow
            }
        };

        if (exception is TicketManagement.Application.Common.Exceptions.ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        // Add stack trace only in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await httpContext.Response.WriteAsync(json, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string Detail, string ErrorCode) MapException(Exception exception)
    {
        return exception switch
        {
            TicketManagement.Application.Common.Exceptions.ValidationException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Validation Error",
                Detail: "One or more validation errors occurred.",
                ErrorCode: "VALIDATION_FAILURE"
            ),
            
            // ✅ NEW: Concurrency exception handling
            TicketManagement.Application.Common.Exceptions.ConcurrencyException concurrencyEx => (
                StatusCode: (int)HttpStatusCode.Conflict,
                Title: "Concurrency Conflict",
                Detail: concurrencyEx.Message,
                ErrorCode: "CONCURRENCY_CONFLICT"
            ),
            
            // 🔥 SENIOR LEVEL: Specific domain exceptions
            TicketNotFoundException ex => (
                StatusCode: (int)HttpStatusCode.NotFound,
                Title: "Ticket Not Found",
                Detail: ex.Message,
                ErrorCode: "TICKET_NOT_FOUND"
            ),
            
            TicketAlreadyClosedException ex => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Ticket Already Closed",
                Detail: ex.Message,
                ErrorCode: "TICKET_ALREADY_CLOSED"
            ),
            
            TicketAssignmentException ex => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Ticket Assignment Error",
                Detail: ex.Message,
                ErrorCode: "TICKET_ASSIGNMENT_ERROR"
            ),
            
            DailyTicketLimitExceededException ex => (
                StatusCode: (int)HttpStatusCode.Forbidden,
                Title: "Daily Limit Exceeded",
                Detail: ex.Message,
                ErrorCode: "DAILY_LIMIT_EXCEEDED"
            ),
            
            BusinessHoursViolationException ex => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Business Hours Violation",
                Detail: ex.Message,
                ErrorCode: "BUSINESS_HOURS_VIOLATION"
            ),
            
            InvalidTicketStateException ex => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Invalid Ticket State",
                Detail: ex.Message,
                ErrorCode: "INVALID_TICKET_STATE"
            ),
            
            UnauthorizedTicketAccessException ex => (
                StatusCode: (int)HttpStatusCode.Forbidden,
                Title: "Unauthorized Access",
                Detail: ex.Message,
                ErrorCode: "UNAUTHORIZED_TICKET_ACCESS"
            ),
            
            CategoryNotFoundException ex => (
                StatusCode: (int)HttpStatusCode.NotFound,
                Title: "Category Not Found",
                Detail: ex.Message,
                ErrorCode: "CATEGORY_NOT_FOUND"
            ),
            
            SlaViolationException ex => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "SLA Violation",
                Detail: ex.Message,
                ErrorCode: "SLA_VIOLATION"
            ),
            
            // 🔥 BIG TECH LEVEL: Rate limiting exception
            RateLimitExceededException rateLimitEx => (
                StatusCode: (int)HttpStatusCode.TooManyRequests,
                Title: "Rate Limit Exceeded",
                Detail: rateLimitEx.Message,
                ErrorCode: "RATE_LIMIT_EXCEEDED"
            ),
            
            // Generic domain exception
            DomainException domainEx => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Domain Rule Violation",
                Detail: domainEx.Message,
                ErrorCode: "DOMAIN_ERROR"
            ),
            
            ArgumentNullException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Missing Required Parameter",
                Detail: "A required parameter was not provided.",
                ErrorCode: "MISSING_PARAMETER"
            ),

            ArgumentException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Invalid Request",
                Detail: "The request contains invalid parameters.",
                ErrorCode: "INVALID_ARGUMENT"
            ),
            
            UnauthorizedAccessException => (
                StatusCode: (int)HttpStatusCode.Unauthorized,
                Title: "Unauthorized",
                Detail: "Authentication is required to access this resource.",
                ErrorCode: "UNAUTHORIZED"
            ),
            
            InvalidOperationException invalidOpEx when invalidOpEx.Message.Contains("not found") => (
                StatusCode: (int)HttpStatusCode.NotFound,
                Title: "Resource Not Found",
                Detail: "The requested resource was not found.",
                ErrorCode: "NOT_FOUND"
            ),
            
            TimeoutException => (
                StatusCode: (int)HttpStatusCode.RequestTimeout,
                Title: "Request Timeout",
                Detail: "The request took too long to process.",
                ErrorCode: "TIMEOUT"
            ),
            
            TaskCanceledException => (
                StatusCode: (int)HttpStatusCode.RequestTimeout,
                Title: "Request Cancelled",
                Detail: "The request was cancelled due to timeout.",
                ErrorCode: "CANCELLED"
            ),
            
            HttpRequestException httpEx => (
                StatusCode: (int)HttpStatusCode.BadGateway,
                Title: "External Service Error",
                Detail: "An external service is currently unavailable.",
                ErrorCode: "EXTERNAL_SERVICE_ERROR"
            ),
            
            _ => (
                StatusCode: (int)HttpStatusCode.InternalServerError,
                Title: "Internal Server Error",
                Detail: "An unexpected error occurred while processing your request.",
                ErrorCode: "INTERNAL_ERROR"
            )
        };
    }

    private static string GetUserId(HttpContext httpContext)
    {
        return httpContext.User?.FindFirst("sub")?.Value 
            ?? httpContext.User?.FindFirst("userId")?.Value 
            ?? "anonymous"; // ✅ Fallback to avoid null
    }
}