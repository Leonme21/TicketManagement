using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManagement.Domain.Common;

/// <summary>
/// ✅ REFACTORED: Base class for Aggregate Roots in DDD
/// Manages domain events and ensures consistency boundaries
/// Single source of truth for domain events (removed from BaseEntity)
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the domain events that have been raised by this aggregate
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be published
    /// </summary>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event
    /// </summary>
    protected void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}