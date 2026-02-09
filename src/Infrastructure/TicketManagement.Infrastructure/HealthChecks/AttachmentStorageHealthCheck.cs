using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketManagement.Infrastructure.Services;

namespace TicketManagement.Infrastructure.HealthChecks;

/// <summary>
/// ✅ Health Check para verificar el estado del almacenamiento de archivos
/// Verifica que el directorio de uploads esté accesible y tenga espacio
/// </summary>
public class AttachmentStorageHealthCheck : IHealthCheck
{
    private readonly AttachmentSettings _settings;
    private readonly ILogger<AttachmentStorageHealthCheck> _logger;

    public AttachmentStorageHealthCheck(
        IOptions<AttachmentSettings> settings,
        ILogger<AttachmentStorageHealthCheck> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var uploadPath = Path.GetFullPath(_settings.UploadPath);
            
            // ✅ Verificar que el directorio existe
            if (!Directory.Exists(uploadPath))
            {
                return new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    $"Upload directory does not exist: {uploadPath}",
                    data: new Dictionary<string, object>
                    {
                        ["upload_path"] = uploadPath,
                        ["directory_exists"] = false,
                        ["last_check"] = DateTime.UtcNow
                    });
            }

            // ✅ Verificar permisos de escritura
            var testFile = Path.Combine(uploadPath, $"health_check_{Guid.NewGuid()}.tmp");
            var canWrite = await TestWritePermissionsAsync(testFile, cancellationToken);

            if (!canWrite)
            {
                return new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    "Cannot write to upload directory",
                    data: new Dictionary<string, object>
                    {
                        ["upload_path"] = uploadPath,
                        ["can_write"] = false,
                        ["last_check"] = DateTime.UtcNow
                    });
            }

            // ✅ Verificar espacio disponible
            var driveInfo = new DriveInfo(Path.GetPathRoot(uploadPath) ?? uploadPath);
            var availableSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            var totalSpaceGB = driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0);
            var usedSpacePercent = ((totalSpaceGB - availableSpaceGB) / totalSpaceGB) * 100;

            var data = new Dictionary<string, object>
            {
                ["upload_path"] = uploadPath,
                ["directory_exists"] = true,
                ["can_write"] = true,
                ["available_space_gb"] = Math.Round(availableSpaceGB, 2),
                ["total_space_gb"] = Math.Round(totalSpaceGB, 2),
                ["used_space_percent"] = Math.Round(usedSpacePercent, 1),
                ["max_file_size_mb"] = _settings.MaxFileSizeBytes / (1024 * 1024),
                ["last_check"] = DateTime.UtcNow
            };

            // ✅ Determinar estado basado en espacio disponible
            var status = DetermineHealthStatus(availableSpaceGB, usedSpacePercent);
            var message = GetHealthMessage(status, availableSpaceGB, usedSpacePercent);

            return new HealthCheckResult(status, message, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Attachment storage health check failed");
            
            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                "Failed to check attachment storage health",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["last_check"] = DateTime.UtcNow
                });
        }
    }

    private async Task<bool> TestWritePermissionsAsync(string testFile, CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Intentar crear y eliminar un archivo de prueba
            await File.WriteAllTextAsync(testFile, "health_check", cancellationToken);
            
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to test write permissions for {TestFile}", testFile);
            return false;
        }
    }

    private static HealthStatus DetermineHealthStatus(double availableSpaceGB, double usedSpacePercent)
    {
        // ✅ Lógica de determinación de estado basada en espacio
        if (availableSpaceGB < 1.0) // Menos de 1GB disponible
        {
            return HealthStatus.Unhealthy;
        }

        if (usedSpacePercent > 90) // Más del 90% usado
        {
            return HealthStatus.Degraded;
        }

        if (usedSpacePercent > 80) // Más del 80% usado
        {
            return HealthStatus.Degraded;
        }

        return HealthStatus.Healthy;
    }

    private static string GetHealthMessage(HealthStatus status, double availableSpaceGB, double usedSpacePercent)
    {
        return status switch
        {
            HealthStatus.Healthy => $"Attachment storage is healthy. Available: {availableSpaceGB:F1}GB, Used: {usedSpacePercent:F1}%",
            HealthStatus.Degraded => $"Attachment storage is degraded. Available: {availableSpaceGB:F1}GB, Used: {usedSpacePercent:F1}%",
            HealthStatus.Unhealthy => $"Attachment storage is unhealthy. Available: {availableSpaceGB:F1}GB",
            _ => "Unknown attachment storage status"
        };
    }
}
