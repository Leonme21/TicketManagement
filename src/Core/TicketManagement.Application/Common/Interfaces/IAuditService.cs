namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Servicio para registrar audit trail de acciones sensibles
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra una acción de auditoría
    /// </summary>
    Task LogActionAsync(
        string action,
        string entityType,
        int entityId,
        int userId,
        object? oldValues = null,
        object? newValues = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra múltiples acciones de auditoría en batch
    /// </summary>
    Task LogActionsAsync(
        IEnumerable<AuditLogEntry> entries,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Entrada de audit log para operaciones batch
/// </summary>
public record AuditLogEntry(
    string Action,
    string EntityType,
    int EntityId,
    int UserId,
    object? OldValues = null,
    object? NewValues = null,
    string? AdditionalInfo = null);