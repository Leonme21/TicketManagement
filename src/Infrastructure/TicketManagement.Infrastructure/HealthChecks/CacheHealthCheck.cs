using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TicketManagement.Infrastructure.HealthChecks;

/// <summary>
/// ✅ NEW: Cache health check for Redis/Memory cache
/// </summary>
public sealed class CacheHealthCheck : IHealthCheck
{
    private readonly IDistributedCache _cache;

    public CacheHealthCheck(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var testKey = $"health-check-{Guid.NewGuid()}";
            var testValue = "test";

            // Try to write to cache
            await _cache.SetStringAsync(testKey, testValue, cancellationToken);

            // Try to read from cache
            var retrievedValue = await _cache.GetStringAsync(testKey, cancellationToken);

            // Clean up
            await _cache.RemoveAsync(testKey, cancellationToken);

            if (retrievedValue == testValue)
            {
                return HealthCheckResult.Healthy(
                    "Cache is healthy",
                    data: new Dictionary<string, object>
                    {
                        ["cacheType"] = _cache.GetType().Name,
                        ["timestamp"] = DateTimeOffset.UtcNow
                    });
            }

            return HealthCheckResult.Degraded(
                "Cache read/write mismatch",
                data: new Dictionary<string, object>
                {
                    ["expected"] = testValue,
                    ["actual"] = retrievedValue ?? "null"
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Cache health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                });
        }
    }
}
