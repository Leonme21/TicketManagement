using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace TicketManagement.Application.Tickets.Commands.UpdateTicket;

/// <summary>
/// 🔥 BIG TECH LEVEL: Update handler with robust concurrency handling
/// Features:
/// - Resource authorization checks before domain operations
/// - Optimistic concurrency control with retry logic
/// - Exponential backoff for retries
/// - Cache invalidation on successful update
/// - Structured logging for debugging
/// - Uses IApplicationDbContext for SaveChanges (not repository)
/// </summary>
public sealed class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly IResourceAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateTicketCommandHandler> _logger;
    
    private const int MaxRetries = 3;
    private const int BaseDelayMs = 100;

    public UpdateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext dbContext,
        IDistributedCache cache,
        IResourceAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger<UpdateTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _dbContext = dbContext;
        _cache = cache;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
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

                // Save changes with concurrency check via DbContext
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Invalidate cache on successful update
                // Using standard cache key pattern
                var cacheKey = $"ticket-{request.TicketId}";
                await _cache.RemoveAsync(cacheKey, cancellationToken);

                _logger.LogInformation("Successfully updated ticket {TicketId} on attempt {Attempt}", 
                    request.TicketId, attempt);

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
            {
                _logger.LogWarning("Concurrency conflict updating ticket {TicketId} on attempt {Attempt}. Retrying...", 
                    request.TicketId, attempt);

                // Exponential backoff
                var delay = TimeSpan.FromMilliseconds(BaseDelayMs * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken);
                
                // Continue to next attempt
            }
            catch (DbUpdateConcurrencyException ex) when (attempt == MaxRetries)
            {
                _logger.LogError(ex, "Failed to update ticket {TicketId} after {MaxRetries} attempts due to concurrency conflicts", 
                    request.TicketId, MaxRetries);
                
                return Result.Conflict("Update failed due to concurrent modifications. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating ticket {TicketId} on attempt {Attempt}", 
                    request.TicketId, attempt);
                throw;
            }
        }

        return Result.InternalError("Update failed after maximum retry attempts");
    }
}