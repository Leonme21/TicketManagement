using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Text.Json;
using TicketManagement.Application.Common.Events;
using TicketManagement.Domain.Common;
using TicketManagement.Infrastructure.Persistence.Outbox;

namespace TicketManagement.Infrastructure.Persistence.Interceptors;

/// <summary>
/// ✅ REFACTORED: Interceptor that handles both immediate publishing and Outbox pattern for domain events
/// - Publishes events immediately via MediatR for synchronous handlers (e.g., cache invalidation)
/// - Stores events in Outbox table for resilience (in case app crashes before event processing)
/// - Background OutboxProcessorService can reprocess failed events
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;
    private static readonly ConcurrentDictionary<Guid, List<IDomainEvent>> _pendingEvents = new();
    
    public OutboxInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        await ProcessAndStoreEventsAsync(eventData.Context, cancellationToken);
        
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        // Note: Event publishing is now handled in ApplicationDbContext.SaveChangesAsync
        // for more reliable execution
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
    
    /// <summary>
    /// Publishes pending domain events immediately
    /// Called by ApplicationDbContext after SaveChanges succeeds
    /// </summary>
    public async Task PublishEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        await PublishPendingEventsAsync(context, cancellationToken);
    }

    private async Task ProcessAndStoreEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        if (!aggregates.Any())
            return;

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .Cast<IDomainEvent>()
            .ToList();

        // ✅ Store events with context ID for later publishing
        var contextId = context.ContextId.InstanceId;
        _pendingEvents[contextId] = domainEvents;

        // ✅ Create outbox messages for resilience (in case app crashes)
        var outboxMessages = domainEvents.Select(evt =>
        {
            var content = JsonSerializer.Serialize(evt, evt.GetType(), new JsonSerializerOptions
            {
                WriteIndented = false
            });

            return new OutboxMessage(evt.GetType().Name, content);
        }).ToList();

        // Add outbox messages to context
        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);

        // Clear domain events from aggregates
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }
    }
    
    private async Task PublishPendingEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        // ✅ Retrieve pending events that were stored before SaveChanges
        var contextId = context.ContextId.InstanceId;
        if (!_pendingEvents.TryRemove(contextId, out var pendingEvents) || !pendingEvents.Any())
        {
            return;
        }
        
        // ✅ Publish all events immediately via MediatR
        foreach (var domainEvent in pendingEvents)
        {
            // Wrap in DomainEventNotification to match handler expectations
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);
            
            await _publisher.Publish(notification!, cancellationToken);
        }
    }
}

