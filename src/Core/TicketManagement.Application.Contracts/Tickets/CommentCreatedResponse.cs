namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// ?? BIG TECH LEVEL: Response DTO for comment creation
/// </summary>
public record CommentCreatedResponse
{
    public required int CommentId { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
