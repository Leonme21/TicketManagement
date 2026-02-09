using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// 🔥 BIG TECH LEVEL: Pipeline behavior for caching
/// Automatic caching for queries with configurable TTL
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly IStructuredLogger _logger;

    public CachingBehavior(IDistributedCache cache, IStructuredLogger logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableRequest cacheableRequest)
            return await next();

        var cacheKey = cacheableRequest.CacheKey;
        
        // Try to get from cache
        var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedResponse))
        {
            _logger.LogPerformanceMetric("CacheHit", TimeSpan.Zero, new { CacheKey = cacheKey });
            return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
        }

        // Execute request
        var response = await next();

        // Cache the response
        if (response != null)
        {
            var serializedResponse = JsonSerializer.Serialize(response);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheableRequest.CacheDuration
            };

            await _cache.SetStringAsync(cacheKey, serializedResponse, cacheOptions, cancellationToken);
            _logger.LogPerformanceMetric("CacheMiss", TimeSpan.Zero, new { CacheKey = cacheKey });
        }

        return response;
    }
}

/// <summary>
/// Marker interface for cacheable requests
/// </summary>
public interface ICacheableRequest
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}

/// <summary>
/// Marker interface for cache invalidation
/// </summary>
public interface ICacheInvalidatingRequest
{
    string[] CacheKeysToInvalidate { get; }
}