using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Caching;

/// <summary>
/// ?? BIG TECH LEVEL: Cache invalidation service implementation
/// Handles all cache invalidation operations
/// Thread-safe and resilient to cache failures
/// </summary>
public sealed class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheInvalidationService> _logger;

    // Using CacheKeys for consistency

    public CacheInvalidationService(
        IDistributedCache cache,
        ILogger<CacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task InvalidateTicketCacheAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var keysToInvalidate = new[]
        {
            CacheKeys.TicketDetails(ticketId),
            // Add other relevant keys if needed, but standardizing on TicketDetails
        };

        await InvalidateKeysAsync(keysToInvalidate, cancellationToken);
        _logger.LogDebug("Invalidated cache for ticket {TicketId}", ticketId);
    }

    public async Task InvalidateTicketListCacheAsync(CancellationToken cancellationToken = default)
    {
        // For distributed cache, we can't easily do pattern-based invalidation
        // In production, use Redis SCAN + DEL or implement a cache versioning strategy
        // For now, we'll invalidate known list cache keys
        
        var keysToInvalidate = new[]
        {
            "tickets:list:recent",
            "tickets:list:popular",
            "tickets:list:unassigned",
            "tickets:count:active"
        };

        await InvalidateKeysAsync(keysToInvalidate, cancellationToken);
        _logger.LogDebug("Invalidated ticket list cache");
    }

    public async Task InvalidateUserTicketsCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        var keysToInvalidate = new[]
        {
            $"user:{userId}:tickets:created",
            $"user:{userId}:tickets:assigned",
            $"user:{userId}:tickets:count"
        };

        await InvalidateKeysAsync(keysToInvalidate, cancellationToken);
        _logger.LogDebug("Invalidated cache for user {UserId} tickets", userId);
    }

    public async Task InvalidateCategoryCacheAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var keysToInvalidate = new[]
        {
            $"category:{categoryId}:tickets",
            $"category:{categoryId}:details",
            "categories:list"
        };

        await InvalidateKeysAsync(keysToInvalidate, cancellationToken);
        _logger.LogDebug("Invalidated cache for category {CategoryId}", categoryId);
    }

    public async Task InvalidateAllCacheAsync(CancellationToken cancellationToken = default)
    {
        // In production with Redis, you would use FLUSHDB or pattern-based deletion
        // For MemoryCache, this is not directly supported
        // Consider implementing a cache version increment strategy instead
        
        _logger.LogWarning("InvalidateAllCache called - this operation may not clear all cache entries with IDistributedCache");
        
        // Invalidate known critical cache keys
        var criticalKeys = new[]
        {
            "tickets:list:recent",
            "tickets:list:popular",
            "tickets:count:active",
            "categories:list"
        };

        await InvalidateKeysAsync(criticalKeys, cancellationToken);
    }

    private async Task InvalidateKeysAsync(string[] keys, CancellationToken cancellationToken)
    {
        var tasks = keys.Select(key => SafeRemoveAsync(key, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task SafeRemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't fail - cache invalidation should be resilient
            _logger.LogWarning(ex, "Failed to remove cache key {CacheKey}", key);
        }
    }
}
