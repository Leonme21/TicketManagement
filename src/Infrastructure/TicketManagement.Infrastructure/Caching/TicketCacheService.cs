using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Tickets;

namespace TicketManagement.Infrastructure.Caching;

/// <summary>
/// 🔥 BIG TECH LEVEL: Intelligent caching service with invalidation strategy
/// Implements ITicketCacheService from Application layer
/// Uses the DTOs defined in ITicketQueryService for consistency
/// </summary>
public sealed class TicketCacheService : ITicketCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ITicketQueryService _queryService;
    private readonly ILogger<TicketCacheService> _logger;
    
    private static readonly TimeSpan SummaryTtl = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DetailsTtl = TimeSpan.FromMinutes(30);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TicketCacheService(
        IDistributedCache cache,
        ITicketQueryService queryService,
        ILogger<TicketCacheService> logger)
    {
        _cache = cache;
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<TicketSummaryDto?> GetTicketSummaryAsync(
        int ticketId, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetSummaryCacheKey(ticketId);
        
        try
        {
            var cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                _logger.LogDebug("Cache hit for ticket summary {TicketId}", ticketId);
                return JsonSerializer.Deserialize<TicketSummaryDto>(cachedJson, JsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for ticket summary {TicketId}", ticketId);
        }

        _logger.LogDebug("Cache miss for ticket summary {TicketId}", ticketId);
        return null;
    }

    public async Task<TicketDetailsDto?> GetTicketDetailsAsync(
        int ticketId, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetDetailsCacheKey(ticketId);
        
        try
        {
            var cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                _logger.LogDebug("Cache hit for ticket details {TicketId}", ticketId);
                return JsonSerializer.Deserialize<TicketDetailsDto>(cachedJson, JsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for ticket details {TicketId}", ticketId);
        }

        _logger.LogDebug("Cache miss for ticket details {TicketId}", ticketId);
        return null;
    }

    public async Task InvalidateTicketCacheAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var tasks = new[]
        {
            _cache.RemoveAsync(GetSummaryCacheKey(ticketId), cancellationToken),
            _cache.RemoveAsync(GetDetailsCacheKey(ticketId), cancellationToken)
        };

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogDebug("Invalidated cache for ticket {TicketId}", ticketId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for ticket {TicketId}", ticketId);
        }
    }

    public async Task InvalidateUserTicketsCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Invalidating user tickets cache for user {UserId}", userId);
        
        try
        {
            await _cache.RemoveAsync($"user:{userId}:tickets", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate user tickets cache for user {UserId}", userId);
        }
    }

    public async Task WarmupPopularTicketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cache warmup for popular tickets");
            
            var filter = new TicketQueryFilter
            {
                SortBy = "CreatedAt",
                SortDescending = true
            };
            
            var recentTickets = await _queryService.GetPaginatedAsync(
                filter, 
                pageNumber: 1, 
                pageSize: 50, 
                cancellationToken);
            
            var warmupTasks = recentTickets.Items.Select(async ticket =>
            {
                var cacheKey = GetSummaryCacheKey(ticket.Id);
                await SetCacheAsync(cacheKey, ticket, SummaryTtl, cancellationToken);
            });
            
            await Task.WhenAll(warmupTasks);
            
            _logger.LogInformation("Cache warmup completed for {Count} tickets", recentTickets.Items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warmup failed");
        }
    }

    private async Task SetCacheAsync<T>(
        string key, 
        T value, 
        TimeSpan ttl, 
        CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            
            await _cache.SetStringAsync(key, json, options, cancellationToken);
            _logger.LogDebug("Cached item with key {CacheKey} for {TTL}", key, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache item with key {CacheKey}", key);
        }
    }

    private static string GetSummaryCacheKey(int ticketId) => $"ticket:summary:{ticketId}:v1";
    private static string GetDetailsCacheKey(int ticketId) => $"ticket:details:{ticketId}:v1";
}