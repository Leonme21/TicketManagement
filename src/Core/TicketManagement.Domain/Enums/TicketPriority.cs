namespace TicketManagement.Domain.Enums;

/// <summary>
/// Nivel de prioridad para resolución de tickets
/// </summary>
public enum TicketPriority
{
    /// <summary>
    /// Baja prioridad - SLA:  7 días
    /// </summary>
    Low = 1,

    /// <summary>
    /// Prioridad media - SLA:  3 días
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Alta prioridad - SLA: 24 horas
    /// </summary>
    High = 3,

    /// <summary>
    /// Crítica - SLA: 4 horas
    /// </summary>
    Critical = 4
}