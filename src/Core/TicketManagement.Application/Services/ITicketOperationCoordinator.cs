using TicketManagement.Application.Tickets.Commands.AddComment;
using TicketManagement.Application.Tickets.Commands.AssignTicket;
using TicketManagement.Application.Tickets.Commands.CloseTicket;
using TicketManagement.Application.Tickets.Commands.CreateTicket;
using TicketManagement.Application.Tickets.Commands.UpdateTicket;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Services;

/// <summary>
/// 🔥 SENIOR LEVEL: Coordinator service to reduce controller dependencies
/// Orchestrates complex ticket operations with proper separation of concerns
/// </summary>
public interface ITicketOperationCoordinator
{
    /// <summary>
    /// Coordinates ticket creation with all necessary validations and side effects
    /// </summary>
    Task<Result<CreateTicketResponse>> CreateTicketAsync(CreateTicketCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Coordinates ticket updates with authorization and business rule validation
    /// </summary>
    Task<Result> UpdateTicketAsync(int ticketId, UpdateTicketCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Coordinates ticket assignment with proper authorization checks
    /// </summary>
    Task<Result> AssignTicketAsync(int ticketId, AssignTicketCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Coordinates ticket closure with SLA tracking and notifications
    /// </summary>
    Task<Result> CloseTicketAsync(int ticketId, CloseTicketCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Coordinates comment addition with rate limiting and authorization
    /// </summary>
    Task<Result<CommentCreatedResponse>> AddCommentAsync(int ticketId, AddCommentCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Response DTOs for coordinator operations
/// </summary>
public record CreateTicketResponse
{
    public required int TicketId { get; init; }
    public required string Message { get; init; }
    public required TimeSpan EstimatedResolutionTime { get; init; }
    public required string Priority { get; init; }
    public required string Status { get; init; }
}

public record CommentCreatedResponse
{
    public required int CommentId { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}