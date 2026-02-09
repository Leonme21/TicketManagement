namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ? BIG TECH LEVEL: Interface for Commands that require idempotency
/// Allows IdempotencyBehavior to apply only to specific commands
/// </summary>
public interface IIdempotentCommand
{
    /// <summary>
    /// Unique key to identify the operation (prevents duplicates)
    /// Client should provide a UUID or similar unique identifier
    /// If null or empty, idempotency check will be skipped
    /// </summary>
    string? IdempotencyKey { get; }
}
