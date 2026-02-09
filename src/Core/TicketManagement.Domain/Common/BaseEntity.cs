using System;
using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Domain.Common;

/// <summary>
/// ✅ REFACTORED: Base class for all domain entities
/// Provides: ID, audit fields (CreatedAt/UpdatedAt/CreatedBy/UpdatedBy)
/// Domain Events removed - only in AggregateRoot
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Fecha de creación (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última modificación (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuario que creó la entidad (UserId o email)
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Usuario que modificó por última vez
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Token de concurrencia optimista (EF Core)
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
