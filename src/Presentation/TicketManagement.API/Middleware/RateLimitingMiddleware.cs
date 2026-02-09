using System.Security.Claims;
using TicketManagement.Infrastructure.Services;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.API.Middleware;

/// <summary>
/// 🔥 BIG TECH LEVEL: Rate limiting middleware for better performance than MediatR behavior
/// Handles rate limiting at the HTTP level with proper headers and responses
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
    {
        // Skip rate limiting for certain paths
        if (ShouldSkipRateLimit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var userId = GetUserId(context);
        if (userId == 0)
        {
            await _next(context);
            return;
        }

        var operationType = GetOperationType(context);
        if (string.IsNullOrEmpty(operationType))
        {
            await _next(context);
            return;
        }

        // Check rate limit
        var rateLimitResult = await rateLimitService.CheckLimitAsync(userId, operationType, context.RequestAborted);

        // Add rate limit headers
        context.Response.Headers.Append("X-RateLimit-Limit", rateLimitResult.Limit.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", rateLimitResult.RemainingRequests.ToString());
        
        if (rateLimitResult.ResetTime.HasValue)
        {
            context.Response.Headers.Append("X-RateLimit-Reset", 
                ((DateTimeOffset)rateLimitResult.ResetTime.Value).ToUnixTimeSeconds().ToString());
        }

        if (!rateLimitResult.IsAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for user {UserId} on operation {Operation}", 
                userId, operationType);

            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Append("Retry-After", 
                rateLimitResult.RetryAfter?.TotalSeconds.ToString() ?? "60");

            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // Record the operation
        await rateLimitService.RecordOperationAsync(userId, operationType, context.RequestAborted);

        await _next(context);
    }

    private static bool ShouldSkipRateLimit(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";
        return pathValue.StartsWith("/health") ||
               pathValue.StartsWith("/swagger") ||
               pathValue.Contains("/auth/");
    }

    private static int GetUserId(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         context.User?.FindFirst("sub")?.Value ??
                         context.User?.FindFirst("userId")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private static string GetOperationType(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        if (path.Contains("/api/tickets") && method == "POST")
            return "create_ticket";

        if (path.Contains("/api/tickets") && method == "PUT")
            return "update_ticket";

        if (path.Contains("/api/tickets") && path.Contains("/comments") && method == "POST")
            return "add_comment";

        if (path.Contains("/api/tickets") && method == "GET")
            return "read_tickets";

        return "";
    }
}