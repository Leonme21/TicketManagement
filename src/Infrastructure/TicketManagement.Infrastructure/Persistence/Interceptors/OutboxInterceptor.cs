using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using TicketManagement.Domain.Common;
using TicketManagement.Infrastructure.Persistence.Outbox;

namespace TicketManagement.Infrastructure.Persistence.Interceptors;

/// <summary>
/// ✅ NEW: Interceptor that handles Outbox pattern for domain events
/// Moved from ApplicationDbContext.SaveChangesAsync for better separation of concerns
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        await ProcessDomainEventsAsync(eventData.Context, cancellationToken);
        
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task ProcessDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
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
            .ToList();

        // Create outbox messages for each domain event
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
}
