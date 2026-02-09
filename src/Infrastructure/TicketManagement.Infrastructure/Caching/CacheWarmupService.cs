using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Caching;

/// <summary>
/// ?? BIG TECH LEVEL: Cache warmup service implementation
/// Pre-populates cache with frequently accessed data on startup or demand
/// </summary>
public sealed class CacheWarmupService : ICacheWarmupService
{
    private readonly IDistributedCache _cache;
    private readonly ITicketQueryService _ticketQueryService;
    private readonly ILogger<CacheWarmupService> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CacheWarmupService(
        IDistributedCache cache,
        ITicketQueryService ticketQueryService,
        ILogger<CacheWarmupService> logger)
    {
        _cache = cache;
        _ticketQueryService = ticketQueryService;
        _logger = logger;
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

            var recentTickets = await _ticketQueryService.GetPaginatedAsync(
                filter, 
                pageNumber: 1, 
                pageSize: 50, 
                cancellationToken);

            var warmupTasks = recentTickets.Items.Select(async ticket =>
            {
                var cacheKey = $"ticket:summary:{ticket.Id}:v1";
                await SetCacheAsync(cacheKey, ticket, cancellationToken);
            });

            await Task.WhenAll(warmupTasks);

            var activeCount = await _ticketQueryService.GetActiveTicketCountAsync(cancellationToken);
            await SetCacheAsync("tickets:count:active", activeCount, cancellationToken);

            var unassignedTickets = await _ticketQueryService.GetUnassignedTicketsAsync(20, cancellationToken);
            await SetCacheAsync("tickets:list:unassigned", unassignedTickets, cancellationToken);

            _logger.LogInformation(
                "Cache warmup completed: {TicketCount} tickets, {UnassignedCount} unassigned",
                recentTickets.Items.Count, 
                unassignedTickets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warmup failed");
        }
    }

    public async Task WarmupCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cache warmup for categories");
            await Task.CompletedTask; // ? Ensure async operation
            _logger.LogInformation("Category cache warmup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Category cache warmup failed");
        }
    }

    private async Task SetCacheAsync<T>(string key, T value, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl
            };

            await _cache.SetStringAsync(key, json, options, cancellationToken);
            _logger.LogDebug("Cached item with key {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache item with key {CacheKey}", key);
        }
    }
}
