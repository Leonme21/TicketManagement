using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.DeleteTicket;

/// <summary>
/// ✅ REFACTORED: Clean handler following Single Responsibility Principle
/// - No manual cache invalidation (soft delete triggers cache cleanup via interceptor/events)
/// - Uses Repository for write operations, DbContext for SaveChanges
/// - Authorization handled in handler (required for resource-based auth)
/// </summary>
public sealed class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IResourceAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTicketCommandHandler> _logger;
    private static readonly ActivitySource ActivitySource = new("TicketManagement.Commands");

    public DeleteTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext dbContext,
        IResourceAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger<DeleteTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("DeleteTicket");
        activity?.SetTag("ticket.id", request.TicketId);

        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.NotFound("Ticket", request.TicketId);
        }

        // Authorization check (resource-based)
        var userId = _currentUserService.UserIdInt ?? 0;
        var canDelete = await _authorizationService.CanDeleteTicketAsync(userId, ticket, cancellationToken);
        if (!canDelete)
        {
            _logger.LogWarning("User {UserId} attempted to delete ticket {TicketId} without authorization", 
                userId, request.TicketId);
            return Result.Forbidden("You do not have permission to delete this ticket.");
        }

        // ✅ Remove via repository (soft delete handled by SoftDeleteInterceptor)
        _ticketRepository.Remove(ticket);
        
        // ✅ Cache invalidation happens automatically:
        // - SoftDeleteInterceptor marks IsDeleted = true
        // - Query filters exclude deleted tickets from cache rebuilds
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} soft deleted successfully", request.TicketId);
        activity?.SetStatus(ActivityStatusCode.Ok);

        return Result.Success();
    }
}
