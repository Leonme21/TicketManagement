using MediatR;
using TicketManagement.Domain.Common;

namespace TicketManagement.Infrastructure.Events;

/// <summary>
/// Wrapper para adaptar DomainEvent del dominio a INotification de MediatR
/// (Adapter Pattern - Infrastructure Concern)
/// Permite mantener el dominio limpio sin dependencias de MediatR
/// </summary>
public sealed class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public TDomainEvent DomainEvent { get; }
}
