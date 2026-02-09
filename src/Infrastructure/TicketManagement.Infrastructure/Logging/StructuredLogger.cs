using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Logging;

/// <summary>
/// 🔥 PRODUCTION-READY: Structured logger implementation with business context
/// Features:
/// - Business operation tracking
/// - Performance monitoring
/// - Security event logging
/// - Correlation ID tracking
/// </summary>
public sealed class StructuredLogger : IStructuredLogger
{
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
    private readonly ILogger<StructuredLogger> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StructuredLogger(ILogger<StructuredLogger> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public IDisposable BeginBusinessScope(string operation, object context)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["BusinessOperation"] = operation,
            ["CorrelationId"] = GetCorrelationId(),
            ["UserId"] = GetCurrentUserId(),
            ["Timestamp"] = DateTimeOffset.UtcNow,
            ["Context"] = context
        };

        return _logger.BeginScope(scopeData) ?? NullScope.Instance;
    }

    public void LogBusinessSuccess(string operation, object? data = null)
    {
        using var scope = CreateOperationScope(operation);
        
        _logger.LogInformation("Business operation completed successfully: {Operation}", operation);
        
        if (data != null)
        {
            _logger.LogDebug("Operation data: {@Data}", data);
        }
    }

    public void LogBusinessFailure(string operation, string reason, object? context = null)
    {
        using var scope = CreateOperationScope(operation);
        
        _logger.LogWarning("Business operation failed: {Operation}. Reason: {Reason}", operation, reason);
        
        if (context != null)
        {
            _logger.LogDebug("Failure context: {@Context}", context);
        }
    }

    public void LogSecurityEvent(string eventType, string description, object? context = null)
    {
        var securityContext = new Dictionary<string, object>
        {
            ["SecurityEventType"] = eventType,
            ["Description"] = description,
            ["UserId"] = GetCurrentUserId(),
            ["IpAddress"] = GetClientIpAddress(),
            ["UserAgent"] = GetUserAgent(),
            ["CorrelationId"] = GetCorrelationId(),
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (context != null)
        {
            securityContext["AdditionalContext"] = context;
        }

        using var scope = _logger.BeginScope(securityContext);
        
        _logger.LogWarning("Security event: {EventType} - {Description}", eventType, description);
    }

    public void LogPerformanceMetric(string operation, TimeSpan duration, object? metadata = null)
    {
        var performanceContext = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["DurationMs"] = duration.TotalMilliseconds,
            ["UserId"] = GetCurrentUserId(),
            ["CorrelationId"] = GetCorrelationId(),
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (metadata != null)
        {
            performanceContext["Metadata"] = metadata;
        }

        using var scope = _logger.BeginScope(performanceContext);
        
        if (duration.TotalMilliseconds > 5000) // Log slow operations as warnings
        {
            _logger.LogWarning("Slow operation detected: {Operation} took {DurationMs}ms", 
                operation, duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogInformation("Operation performance: {Operation} completed in {DurationMs}ms", 
                operation, duration.TotalMilliseconds);
        }
    }

    private IDisposable CreateOperationScope(string operation)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["CorrelationId"] = GetCorrelationId(),
            ["UserId"] = GetCurrentUserId(),
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        return _logger.BeginScope(scopeData) ?? NullScope.Instance;
    }

    private string GetCorrelationId()
    {
        return _httpContextAccessor.HttpContext?.TraceIdentifier ?? Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }

    private string GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.FindFirst("sub")?.Value 
            ?? httpContext?.User?.FindFirst("userId")?.Value 
            ?? "anonymous"; // ✅ Fallback to avoid null
    }

    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "unknown";

        var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = ipAddress.Split(',')[0].Trim();
        }

        return ipAddress ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"; // ✅ Fallback
    }

    private string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "unknown"; // ✅ Fallback
    }
}