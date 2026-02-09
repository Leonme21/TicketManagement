using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Events;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Events;

namespace TicketManagement.Application.Tickets.EventHandlers;

/// <summary>
/// ?? BIG TECH LEVEL: Event-driven cache invalidation
/// Handles cache invalidation for all ticket-related domain events
/// This decouples cache logic from controllers and handlers
/// </summary>
public sealed class TicketCacheInvalidationHandler :
    INotificationHandler<DomainEventNotification<TicketCreatedEvent>>,
    INotificationHandler<DomainEventNotification<TicketUpdatedEvent>>,
    INotificationHandler<DomainEventNotification<TicketAssignedEvent>>,
    INotificationHandler<DomainEventNotification<TicketClosedEvent>>,
    INotificationHandler<DomainEventNotification<TicketCommentAddedEvent>>
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly ILogger<TicketCacheInvalidationHandler> _logger;

    public TicketCacheInvalidationHandler(
        ICacheInvalidationService cacheInvalidationService,
        ILogger<TicketCacheInvalidationHandler> logger)
    {
        _cacheInvalidationService = cacheInvalidationService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<TicketCreatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogDebug("Invalidating cache for newly created ticket {TicketId}", evt.TicketId);
        
        await _cacheInvalidationService.InvalidateTicketCacheAsync(evt.TicketId, ct);
        await _cacheInvalidationService.InvalidateTicketListCacheAsync(ct);
    }

    public async Task Handle(DomainEventNotification<TicketUpdatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogDebug("Invalidating cache for updated ticket {TicketId}", evt.TicketId);
        
        await _cacheInvalidationService.InvalidateTicketCacheAsync(evt.TicketId, ct);
    }

    public async Task Handle(DomainEventNotification<TicketAssignedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogDebug("Invalidating cache for assigned ticket {TicketId}", evt.TicketId);
        
        await _cacheInvalidationService.InvalidateTicketCacheAsync(evt.TicketId, ct);
        
        // Invalidate agent's ticket list cache
        if (evt.PreviousAgentId.HasValue)
        {
            await _cacheInvalidationService.InvalidateUserTicketsCacheAsync(evt.PreviousAgentId.Value, ct);
        }
        await _cacheInvalidationService.InvalidateUserTicketsCacheAsync(evt.NewAgentId, ct);
    }

    public async Task Handle(DomainEventNotification<TicketClosedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogDebug("Invalidating cache for closed ticket {TicketId}", evt.TicketId);
        
        await _cacheInvalidationService.InvalidateTicketCacheAsync(evt.TicketId, ct);
        await _cacheInvalidationService.InvalidateTicketListCacheAsync(ct);
    }

    public async Task Handle(DomainEventNotification<TicketCommentAddedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogDebug("Invalidating cache for ticket {TicketId} after comment added", evt.TicketId);
        
        await _cacheInvalidationService.InvalidateTicketCacheAsync(evt.TicketId, ct);
    }
}
