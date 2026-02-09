using TicketManagement.Infrastructure.Persistence.Outbox;

namespace TicketManagement.Infrastructure.Persistence.Outbox;

/// <summary>
/// Service for manaing the Outbox
/// </summary>
public interface IOutboxService
{
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid id, string error, CancellationToken cancellationToken = default);
}
