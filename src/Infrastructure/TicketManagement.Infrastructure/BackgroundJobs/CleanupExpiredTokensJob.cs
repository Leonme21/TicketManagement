using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Infrastructure.BackgroundJobs;

/// <summary>
/// ✅ Background Job para limpieza automática de tokens expirados
/// Ejecuta cada 6 horas para mantener la base de datos limpia
/// </summary>
public class CleanupExpiredTokensJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanupExpiredTokensJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    // ✅ Logging estructurado
    private static readonly Action<ILogger, int, Exception?> _logTokensCleanedUp =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(5001, "TokensCleanedUp"),
            "Cleaned up {Count} expired refresh tokens");

    private static readonly Action<ILogger, Exception?> _logCleanupStarted =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(5002, "CleanupStarted"),
            "Starting cleanup of expired refresh tokens");

    private static readonly Action<ILogger, Exception?> _logCleanupCompleted =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(5003, "CleanupCompleted"),
            "Cleanup of expired refresh tokens completed");

    public CleanupExpiredTokensJob(
        IServiceProvider serviceProvider,
        ILogger<CleanupExpiredTokensJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🧹 CleanupExpiredTokensJob started, running every {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during token cleanup");
            }

            // ✅ Esperar el intervalo antes del próximo ciclo
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        _logCleanupStarted(_logger, null);

        // ✅ Crear scope para obtener servicios scoped
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // ✅ Eliminar tokens expirados
            var deletedCount = await unitOfWork.RefreshTokens.DeleteExpiredTokensAsync(cancellationToken);

            if (deletedCount > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                _logTokensCleanedUp(_logger, deletedCount, null);
            }
            else
            {
                _logger.LogDebug("No expired tokens found to clean up");
            }

            _logCleanupCompleted(_logger, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired tokens");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 CleanupExpiredTokensJob is stopping");
        await base.StopAsync(cancellationToken);
    }
}
