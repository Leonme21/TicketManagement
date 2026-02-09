using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// 🔥 PRODUCTION-READY: Comprehensive ticket summary DTO for list views
/// </summary>
public record TicketSummaryDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required TicketStatus Status { get; init; }
    public required TicketPriority Priority { get; init; }
    public required string CategoryName { get; init; }
    public required string CreatorName { get; init; }
    public string? AssignedToName { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public int CommentCount { get; init; }
    public bool IsOverdue { get; init; }
    public TimeSpan? TimeToResolution { get; init; }
}