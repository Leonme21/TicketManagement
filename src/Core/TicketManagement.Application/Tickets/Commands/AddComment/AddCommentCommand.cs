using MediatR;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Common.Behaviors;

namespace TicketManagement.Application.Tickets.Commands.AddComment;

/// <summary>
/// ✅ BIG TECH LEVEL: Command for adding comments to tickets with rate limiting
/// </summary>
public record AddCommentCommand : IRequest<Result<int>>, IRateLimitedRequest
{
    public int TicketId { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsInternal { get; init; } = false;
    
    /// <summary>
    /// Rate limiting operation type for comment creation
    /// </summary>
    public string OperationType => "CommentCreation";
}
