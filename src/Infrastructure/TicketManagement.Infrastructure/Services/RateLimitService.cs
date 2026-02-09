using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Services;

public sealed class RateLimitService : IRateLimitService
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateLimitService> _logger;

    public RateLimitService(IMemoryCache cache, IConfiguration configuration, ILogger<RateLimitService> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RateLimitResult> CheckLimitAsync(int userId, string operationType, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // ? Ensure proper async handling
        
        var limits = GetLimitsForOperation(operationType);
        var cacheKey = $"rate_limit:{userId}:{operationType}";
        
        var requests = _cache.Get<List<DateTime>>(cacheKey) ?? new List<DateTime>();
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(limits.Window);
        
        // Remove old requests outside the window
        requests = requests.Where(r => r > windowStart).ToList();
        
        var isAllowed = requests.Count < limits.MaxRequests;
        var remaining = Math.Max(0, limits.MaxRequests - requests.Count);
        var resetTime = requests.Count > 0 ? requests.Min().Add(limits.Window) : now.Add(limits.Window);
        
        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            RemainingRequests = remaining,
            Limit = limits.MaxRequests,
            RetryAfter = isAllowed ? null : resetTime.Subtract(now),
            ResetTime = resetTime
        };
    }

    public async Task RecordOperationAsync(int userId, string operationType, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // ? Ensure proper async handling
        
        var limits = GetLimitsForOperation(operationType);
        var cacheKey = $"rate_limit:{userId}:{operationType}";
        
        var requests = _cache.Get<List<DateTime>>(cacheKey) ?? new List<DateTime>();
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(limits.Window);
        
        // Remove old requests and add current request
        requests = requests.Where(r => r > windowStart).ToList();
        requests.Add(now);
        
        _cache.Set(cacheKey, requests, limits.Window.Add(TimeSpan.FromMinutes(1)));
        
        _logger.LogDebug("Recorded operation {Operation} for user {UserId}. Current count: {Count}/{Limit}", 
            operationType, userId, requests.Count, limits.MaxRequests);
    }

    private RateLimitConfiguration GetLimitsForOperation(string operationType)
    {
        return operationType switch
        {
            "create_ticket" => new RateLimitConfiguration(
                MaxRequests: _configuration.GetValue("RateLimit:CreateTicket:MaxRequests", 10),
                Window: TimeSpan.FromMinutes(_configuration.GetValue("RateLimit:CreateTicket:WindowMinutes", 60))
            ),
            "update_ticket" => new RateLimitConfiguration(
                MaxRequests: _configuration.GetValue("RateLimit:UpdateTicket:MaxRequests", 20),
                Window: TimeSpan.FromMinutes(_configuration.GetValue("RateLimit:UpdateTicket:WindowMinutes", 60))
            ),
            "add_comment" => new RateLimitConfiguration(
                MaxRequests: _configuration.GetValue("RateLimit:AddComment:MaxRequests", 30),
                Window: TimeSpan.FromMinutes(_configuration.GetValue("RateLimit:AddComment:WindowMinutes", 60))
            ),
            "read_tickets" => new RateLimitConfiguration(
                MaxRequests: _configuration.GetValue("RateLimit:ReadTickets:MaxRequests", 100),
                Window: TimeSpan.FromMinutes(_configuration.GetValue("RateLimit:ReadTickets:WindowMinutes", 60))
            ),
            _ => new RateLimitConfiguration(MaxRequests: 60, Window: TimeSpan.FromHours(1))
        };
    }
}

public record RateLimitConfiguration(int MaxRequests, TimeSpan Window);