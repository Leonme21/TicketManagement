using System;

namespace TicketManagement.Infrastructure.Persistence.Outbox;

/// <summary>
/// Mensaje pendiente de procesar por el Outbox Pattern
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty; // JSON Content
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }

    public OutboxMessage(string type, string data)
    {
        Id = Guid.NewGuid();
        Type = type;
        Data = data;
        CreatedAt = DateTime.UtcNow;
    }

    // Constructor required by EF Core
    private OutboxMessage() { }
}
