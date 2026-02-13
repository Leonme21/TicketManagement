using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// Response DTO for ticket creation
/// </summary>
public record CreateTicketResponse
{
    public required int TicketId { get; init; }
    public required string Message { get; init; }
    public TimeSpan? EstimatedResolutionTime { get; init; }
    public required TicketPriority Priority { get; init; }
    public required TicketStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
