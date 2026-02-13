using MediatR;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Common.Behaviors;
using TicketManagement.Application.Contracts.Tickets;

namespace TicketManagement.Application.Tickets.Commands.CreateTicket;

/// <summary>
/// ✅ BIG TECH LEVEL: Command with idempotency and rate limiting support
/// - ICommand: Explicit transaction detection
/// - IIdempotentCommand: Prevents duplicate ticket creation
/// - IRateLimitedRequest: Rate limiting for abuse prevention
/// </summary>
public record CreateTicketCommand : ICommand<CreateTicketResponse>, IIdempotentCommand, IRateLimitedRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required TicketPriority Priority { get; init; }
    public required int CategoryId { get; init; }
    
    /// <summary>
    /// ✅ Idempotency key to prevent duplicate ticket creation
    /// Client should provide a unique key (e.g., UUID) for each unique request
    /// </summary>
    public string? IdempotencyKey { get; init; }
    
    /// <summary>
    /// ✅ Rate limiting operation type
    /// </summary>
    public string OperationType => "TicketCreation";
}