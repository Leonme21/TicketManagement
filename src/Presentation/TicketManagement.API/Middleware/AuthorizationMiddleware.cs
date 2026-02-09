using System.Security.Claims;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.API.Middleware;

/// <summary>
/// 🔥 BIG TECH LEVEL: Authorization middleware for better performance than MediatR behavior
/// Handles authorization at the HTTP level before reaching the application layer
/// </summary>
public sealed class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService)
    {
        // Skip authorization for certain paths
        if (ShouldSkipAuthorization(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Extract user information
        var userId = GetUserId(context);
        if (userId == 0)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Determine resource and action from the request
        var (resource, action) = ExtractResourceAndAction(context);
        
        if (!string.IsNullOrEmpty(resource) && !string.IsNullOrEmpty(action))
        {
            var isAuthorized = await authorizationService.IsAuthorizedAsync(
                userId, resource, action, context.RequestAborted);

            if (!isAuthorized)
            {
                _logger.LogWarning("Authorization failed for user {UserId} on resource {Resource} with action {Action}", 
                    userId, resource, action);
                
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
                return;
            }
        }

        await _next(context);
    }

    private static bool ShouldSkipAuthorization(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";
        return pathValue.StartsWith("/health") ||
               pathValue.StartsWith("/swagger") ||
               pathValue.StartsWith("/api/auth/login") ||
               pathValue.StartsWith("/api/auth/register");
    }

    private static int GetUserId(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         context.User?.FindFirst("sub")?.Value ??
                         context.User?.FindFirst("userId")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private static (string resource, string action) ExtractResourceAndAction(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        // Extract resource from path
        if (path.Contains("/api/tickets"))
        {
            var action = method switch
            {
                "GET" => "read",
                "POST" => "create",
                "PUT" => "update",
                "DELETE" => "delete",
                _ => "unknown"
            };

            // Special cases for ticket operations
            if (path.Contains("/assign"))
                action = "assign";
            else if (path.Contains("/close"))
                action = "close";
            else if (path.Contains("/comments"))
                action = "comment";

            return ("ticket", action);
        }

        if (path.Contains("/api/categories"))
            return ("category", method == "GET" ? "read" : "manage");

        if (path.Contains("/api/users"))
            return ("user", method == "GET" ? "read" : "manage");

        return ("", "");
    }
}