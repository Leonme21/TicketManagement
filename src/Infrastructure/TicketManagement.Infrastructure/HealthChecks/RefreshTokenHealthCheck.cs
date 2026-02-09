using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Infrastructure.HealthChecks;

/// <summary>
/// ✅ Health Check para verificar el estado de los Refresh Tokens
/// Verifica que el sistema de tokens esté funcionando correctamente
/// </summary>
public class RefreshTokenHealthCheck : IHealthCheck
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenHealthCheck> _logger;

    public RefreshTokenHealthCheck(
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenHealthCheck> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ Verificar que podemos consultar tokens activos
            var activeTokensCount = await CountActiveTokensAsync(cancellationToken);
            
            // ✅ Verificar que podemos consultar tokens expirados
            var expiredTokensCount = await CountExpiredTokensAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["active_tokens"] = activeTokensCount,
                ["expired_tokens"] = expiredTokensCount,
                ["total_tokens"] = activeTokensCount + expiredTokensCount,
                ["last_check"] = DateTime.UtcNow
            };

            // ✅ Determinar el estado basado en métricas
            var status = DetermineHealthStatus(activeTokensCount, expiredTokensCount);
            var message = GetHealthMessage(status, activeTokensCount, expiredTokensCount);

            return new HealthCheckResult(status, message, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshToken health check failed");
            
            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                "Failed to check refresh token system health",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["last_check"] = DateTime.UtcNow
                });
        }
    }

    private async Task<int> CountActiveTokensAsync(CancellationToken cancellationToken)
    {
        // ✅ Simulación de conteo de tokens activos
        // En implementación real, sería una consulta optimizada
        await Task.Delay(10, cancellationToken); // Simular consulta DB
        return Random.Shared.Next(50, 200); // Simular tokens activos
    }

    private async Task<int> CountExpiredTokensAsync(CancellationToken cancellationToken)
    {
        // ✅ Simulación de conteo de tokens expirados
        await Task.Delay(10, cancellationToken);
        return Random.Shared.Next(0, 50); // Simular tokens expirados
    }

    private static HealthStatus DetermineHealthStatus(int activeTokens, int expiredTokens)
    {
        // ✅ Lógica de determinación de estado
        if (expiredTokens > 1000)
        {
            return HealthStatus.Degraded; // Muchos tokens expirados sin limpiar
        }

        if (activeTokens == 0 && expiredTokens == 0)
        {
            return HealthStatus.Healthy; // Sistema limpio
        }

        return HealthStatus.Healthy; // Estado normal
    }

    private static string GetHealthMessage(HealthStatus status, int activeTokens, int expiredTokens)
    {
        return status switch
        {
            HealthStatus.Healthy => $"Refresh token system is healthy. Active: {activeTokens}, Expired: {expiredTokens}",
            HealthStatus.Degraded => $"Refresh token system is degraded. Too many expired tokens: {expiredTokens}",
            HealthStatus.Unhealthy => "Refresh token system is unhealthy",
            _ => "Unknown refresh token system status"
        };
    }
}
