using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.BackgroundServices;

/// <summary>
/// 🔥 STAFF LEVEL: Background service for cache warmup on application startup
/// Removes maintenance responsibility from Controllers (SRP violation fix)
/// Controllers should be pure HTTP adapters, not maintenance orchestrators
/// </summary>
public sealed class CacheWarmupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmupBackgroundService> _logger;

    public CacheWarmupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CacheWarmupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for application to fully start
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        _logger.LogInformation("🔥 Cache warmup background service starting...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetService<ITicketCacheService>();

            if (cacheService != null)
            {
                await cacheService.WarmupPopularTicketsAsync(stoppingToken);
                _logger.LogInformation("✅ Cache warmup completed successfully");
            }
            else
            {
                _logger.LogWarning("⚠️ ITicketCacheService not registered, skipping cache warmup");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Cache warmup failed");
            // Don't throw - warmup failure shouldn't crash the application
        }
    }
}
