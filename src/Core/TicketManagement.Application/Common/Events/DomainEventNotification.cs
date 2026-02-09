using MediatR;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Events;

/// <summary>
/// ? STAFF LEVEL: Wrapper para Domain Events compatible con MediatR
/// Permite que Application Layer maneje eventos sin depender de Infrastructure
/// Usa IDomainEvent (interfaz) en lugar de DomainEvent (clase concreta) para mayor flexibilidad
/// </summary>
public sealed class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
