using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Infrastructure.Persistence;

namespace TicketManagement.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de auditoría
/// Registra todas las acciones sensibles en la base de datos
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogActionAsync(
        string action,
        string entityType,
        int entityId,
        int userId,
        object? oldValues = null,
        object? newValues = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog(
                action,
                entityType,
                entityId,
                userId,
                SerializeObject(oldValues),
                SerializeObject(newValues),
                additionalInfo);

            // Agregar información de contexto HTTP si está disponible
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext.Request.Headers.UserAgent.ToString();
                auditLog.SetHttpContext(ipAddress, userAgent);
            }

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Audit log created: {Action} on {EntityType} {EntityId} by user {UserId}",
                action, entityType, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to create audit log for action {Action} on {EntityType} {EntityId}",
                action, entityType, entityId);
            // No re-throw para evitar que falle la operación principal
        }
    }

    public async Task LogActionsAsync(
        IEnumerable<AuditLogEntry> entries,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = new List<AuditLog>();
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext != null ? GetClientIpAddress(httpContext) : string.Empty;
            var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;

            foreach (var entry in entries)
            {
                var auditLog = new AuditLog(
                    entry.Action,
                    entry.EntityType,
                    entry.EntityId,
                    entry.UserId,
                    SerializeObject(entry.OldValues),
                    SerializeObject(entry.NewValues),
                    entry.AdditionalInfo);

                auditLog.SetHttpContext(ipAddress, userAgent);
                auditLogs.Add(auditLog);
            }

            _context.AuditLogs.AddRange(auditLogs);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch audit log created with {Count} entries", auditLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create batch audit logs");
            // No re-throw para evitar que falle la operación principal
        }
    }

    private static string? SerializeObject(object? obj)
    {
        if (obj == null) return null;

        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch
        {
            return obj.ToString();
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Intentar obtener la IP real considerando proxies
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            // X-Forwarded-For puede contener múltiples IPs, tomar la primera
            ipAddress = ipAddress.Split(',')[0].Trim();
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
        }

        return ipAddress ?? "Unknown";
    }
}