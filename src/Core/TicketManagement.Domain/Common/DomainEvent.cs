namespace TicketManagement.Domain.Common;

/// <summary>
/// Interfaz marcadora para eventos de dominio (DDD pattern)
/// NO depende de ningún framework externo - Pure Domain
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Fecha y hora en que ocurrió el evento (UTC)
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}

/// <summary>
/// Clase base abstracta para todos los eventos de dominio.
/// Implementación pura sin dependencias externas (Clean Architecture)
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc />
    public Guid EventId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; init; }
}
