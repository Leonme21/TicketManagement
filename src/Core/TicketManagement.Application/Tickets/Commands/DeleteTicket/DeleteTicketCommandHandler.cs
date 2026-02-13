using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.DeleteTicket;

/// <summary>
/// ?? BIG TECH LEVEL: Handler with cache invalidation and proper CQRS
/// Uses Repository for write operations, DbContext for SaveChanges
/// </summary>
public sealed class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly IResourceAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTicketCommandHandler> _logger;
    private static readonly ActivitySource ActivitySource = new("TicketManagement.Commands");

    public DeleteTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext dbContext,
        ICacheService cache,
        IResourceAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger<DeleteTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _dbContext = dbContext;
        _cache = cache;
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

        // Authorization check
        var userId = _currentUserService.UserIdInt ?? 0;
        var canDelete = await _authorizationService.CanDeleteTicketAsync(userId, ticket, cancellationToken);
        if (!canDelete)
        {
            _logger.LogWarning("User {UserId} attempted to delete ticket {TicketId} without authorization", userId, request.TicketId);
            return Result.Forbidden("You do not have permission to delete this ticket.");
        }

        // Remove via repository (soft delete handled by interceptor)
        _ticketRepository.Remove(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // ?? CRITICAL: Invalidate cache after deletion
        await _cache.RemoveAsync(CacheKeys.TicketDetails(request.TicketId), cancellationToken);
        _logger.LogDebug("Cache invalidated for deleted ticket {TicketId}", request.TicketId);

        _logger.LogInformation("Ticket {TicketId} soft deleted successfully", request.TicketId);
        activity?.SetStatus(ActivityStatusCode.Ok);

        return Result.Success();
    }
}
