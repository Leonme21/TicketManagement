using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TicketManagement.Domain.Common;

namespace TicketManagement.Infrastructure.Persistence.Outbox;

/// <summary>
/// 🔥 BIG TECH LEVEL: Outbox service for reliable event processing
/// </summary>
public class OutboxService : IOutboxService
{
    private readonly ApplicationDbContext _context;

    public OutboxService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var outboxEvents = domainEvents.Select(domainEvent => new OutboxEvent
        {
            EventType = domainEvent.GetType().Name,
            EventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            CreatedAt = DateTime.UtcNow
        });

        _context.OutboxEvents.AddRange(outboxEvents);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var events = await _context.OutboxEvents
            .Where(e => !e.Processed && e.RetryCount < 5)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        return events.Select(e => new OutboxMessage(e.EventType, e.EventData)
        {
            Id = e.Id,
            CreatedAt = e.CreatedAt,
            ProcessedAt = e.ProcessedAt,
            Error = e.Error
        }).ToList();
    }

    public async Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] { eventId }, cancellationToken);
        if (outboxEvent != null)
        {
            outboxEvent.Processed = true;
            outboxEvent.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default)
    {
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] { eventId }, cancellationToken);
        if (outboxEvent != null)
        {
            outboxEvent.RetryCount++;
            outboxEvent.Error = error;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}