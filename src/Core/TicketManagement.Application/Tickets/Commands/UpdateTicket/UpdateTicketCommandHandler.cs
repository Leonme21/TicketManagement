using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.UpdateTicket;

/// <summary>
/// ✅ REFACTORED: Clean handler following Single Responsibility Principle
/// - No infrastructure exception handling (handled by TransactionBehavior)
/// - No manual cache invalidation (handled by TicketCacheInvalidationHandler via events)
/// - No manual authorization (handled by ResourceAuthorizationService in controller/behavior)
/// - Focuses solely on orchestrating domain logic
/// - TransactionBehavior handles DbUpdateConcurrencyException and retries
/// </summary>
public sealed class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IResourceAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateTicketCommandHandler> _logger;

    public UpdateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext dbContext,
        IResourceAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger<UpdateTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        // Retrieve ticket
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            return Result.NotFound("Ticket", request.TicketId);
        }

        // Resource authorization check
        var userId = _currentUserService.UserIdInt ?? 0;
        var canUpdate = await _authorizationService.CanUpdateTicketAsync(userId, ticket, cancellationToken);
        
        if (!canUpdate)
        {
            _logger.LogWarning("User {UserId} attempted to update ticket {TicketId} without authorization", 
                userId, request.TicketId);
            return Result.Forbidden("You do not have permission to update this ticket.");
        }

        // Verify row version for optimistic concurrency
        if (!ticket.RowVersion.SequenceEqual(request.RowVersion))
        {
            _logger.LogWarning("Ticket {TicketId} has been modified by another user", request.TicketId);
            return Result.Conflict("The ticket has been modified by another user. Please refresh and try again.");
        }

        // Apply business logic update
        var updateResult = ticket.Update(request.Title, request.Description, request.Priority);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        // Update category if changed
        if (request.CategoryId != ticket.CategoryId)
        {
            var categoryResult = ticket.ChangeCategory(request.CategoryId);
            if (categoryResult.IsFailure)
            {
                return categoryResult;
            }
        }

        // ✅ Save changes - TransactionBehavior handles DbUpdateConcurrencyException and retries
        // ✅ ticket.Update() emits TicketUpdatedEvent
        // ✅ TicketCacheInvalidationHandler invalidates cache automatically via event
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated ticket {TicketId}", request.TicketId);

        return Result.Success();
    }
}