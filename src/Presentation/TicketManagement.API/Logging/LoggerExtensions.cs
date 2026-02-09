using Microsoft.Extensions.Logging;

namespace TicketManagement.WebApi.Logging;

/// <summary>
/// ? Logging estructurado de alto rendimiento con LoggerMessage.Define
/// Evita allocations y boxing de parámetros
/// </summary>
public static partial class LoggerExtensions
{
    // ==================== AUTHENTICATION ====================
    
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "User login attempt for email: {Email}")]
    public static partial void LogLoginAttempt(this ILogger logger, string email);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "User {UserId} logged in successfully")]
    public static partial void LogLoginSuccess(this ILogger logger, int userId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "Failed login attempt for email: {Email}")]
    public static partial void LogLoginFailed(this ILogger logger, string email);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message = "User registered: {Email}")]
    public static partial void LogUserRegistered(this ILogger logger, string email);

    // ==================== TICKETS ====================
    
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Ticket {TicketId} created by user {UserId}")]
    public static partial void LogTicketCreated(this ILogger logger, int ticketId, int userId);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Ticket {TicketId} assigned to agent {AgentId}")]
    public static partial void LogTicketAssigned(this ILogger logger, int ticketId, int agentId);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Information,
        Message = "Ticket {TicketId} closed by user {UserId}")]
    public static partial void LogTicketClosed(this ILogger logger, int ticketId, int userId);

    // ==================== ERRORS ====================
    
    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Internal server error occurred. CorrelationId: {CorrelationId}")]
    public static partial void LogInternalError(this ILogger logger, string correlationId, Exception exception);

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Warning,
        Message = "Domain validation failed: {Message}")]
    public static partial void LogDomainValidationFailed(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Warning,
        Message = "Resource not found: {ResourceType} with ID {ResourceId}")]
    public static partial void LogResourceNotFound(this ILogger logger, string resourceType, string resourceId);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Warning,
        Message = "Forbidden access attempt by user {UserId} to resource {ResourceType} {ResourceId}")]
    public static partial void LogForbiddenAccess(this ILogger logger, int userId, string resourceType, string resourceId);

    // ==================== PERFORMANCE ====================
    
    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Warning,
        Message = "Long running request: {RequestName} took {ElapsedMilliseconds}ms")]
    public static partial void LogLongRunningRequest(this ILogger logger, string requestName, long elapsedMilliseconds);

    // ==================== BACKGROUND JOBS ====================
    
    [LoggerMessage(
        EventId = 7001,
        Level = LogLevel.Information,
        Message = "Processing {MessageCount} outbox messages")]
    public static partial void LogProcessingOutboxMessages(this ILogger logger, int messageCount);

    [LoggerMessage(
        EventId = 7002,
        Level = LogLevel.Error,
        Message = "Failed to process outbox message {MessageId}. Retry attempt {RetryCount}")]
    public static partial void LogOutboxMessageFailed(this ILogger logger, Guid messageId, int retryCount, Exception exception);

    [LoggerMessage(
        EventId = 7003,
        Level = LogLevel.Warning,
        Message = "Outbox processing job encountered error. Backing off for {BackoffSeconds} seconds")]
    public static partial void LogOutboxBackoff(this ILogger logger, int backoffSeconds, Exception exception);
}
