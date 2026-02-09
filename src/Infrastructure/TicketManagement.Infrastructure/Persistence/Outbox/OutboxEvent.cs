namespace TicketManagement.Infrastructure.Persistence.Outbox;

/// <summary>
/// 🔥 BIG TECH LEVEL: Outbox pattern for reliable event processing
/// Ensures eventual consistency and reliable message delivery
/// </summary>
public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? Error { get; set; }
}