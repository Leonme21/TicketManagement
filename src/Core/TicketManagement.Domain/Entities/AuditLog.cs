using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Entidad para audit trail de acciones sensibles
/// Registra todas las operaciones críticas del sistema
/// </summary>
public class AuditLog : BaseEntity
{
    private AuditLog() { } // EF Core

    public AuditLog(
        string action,
        string entityType,
        int entityId,
        int userId,
        string? oldValues = null,
        string? newValues = null,
        string? additionalInfo = null)
    {
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        UserId = userId;
        OldValues = oldValues;
        NewValues = newValues;
        AdditionalInfo = additionalInfo;
        Timestamp = DateTime.UtcNow;
        IpAddress = string.Empty; // Se establecerá desde la infraestructura
        UserAgent = string.Empty; // Se establecerá desde la infraestructura
    }

    /// <summary>
    /// Acción realizada (Create, Update, Delete, Assign, etc.)
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo de entidad afectada (Ticket, User, Category, etc.)
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>
    /// ID de la entidad afectada
    /// </summary>
    public int EntityId { get; private set; }

    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Valores anteriores (JSON serializado)
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// Valores nuevos (JSON serializado)
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// Información adicional sobre la acción
    /// </summary>
    public string? AdditionalInfo { get; private set; }

    /// <summary>
    /// Timestamp de la acción
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Dirección IP del usuario
    /// </summary>
    public string IpAddress { get; private set; } = string.Empty;

    /// <summary>
    /// User Agent del navegador
    /// </summary>
    public string UserAgent { get; private set; } = string.Empty;

    /// <summary>
    /// Usuario que realizó la acción (navegación)
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Establece información de contexto HTTP
    /// </summary>
    public void SetHttpContext(string ipAddress, string userAgent)
    {
        IpAddress = ipAddress ?? string.Empty;
        UserAgent = userAgent ?? string.Empty;
    }
}