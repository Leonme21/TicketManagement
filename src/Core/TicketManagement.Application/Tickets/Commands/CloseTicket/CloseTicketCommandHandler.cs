using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.CloseTicket;

/// <summary>
/// ?? BIG TECH LEVEL: Close ticket command handler with cache invalidation
/// Uses Repository for aggregate operations, DbContext for SaveChanges
/// </summary>
public sealed class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly ILogger<CloseTicketCommandHandler> _logger;
    private static readonly ActivitySource ActivitySource = new("TicketManagement.Commands");

    public CloseTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext dbContext,
        ICacheService cache,
        ILogger<CloseTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("CloseTicket");
        activity?.SetTag("ticket.id", request.TicketId);

        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null)
            {
                _logger.LogWarning("Ticket {TicketId} not found for closing", request.TicketId);
                return Result.NotFound("Ticket", request.TicketId);
            }

            // ?? Close ticket using domain method (domain event emitted)
            var closeResult = ticket.Close();
            if (closeResult.IsFailure)
            {
                _logger.LogWarning("Failed to close ticket {TicketId}: {Error}",
                    request.TicketId, closeResult.Error);
                return closeResult;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // ?? CRITICAL: Invalidate cache after status change
            await _cache.RemoveAsync(CacheKeys.TicketDetails(request.TicketId), cancellationToken);
            _logger.LogDebug("Cache invalidated for ticket {TicketId}", request.TicketId);

            _logger.LogInformation("Ticket {TicketId} closed successfully", request.TicketId);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close ticket {TicketId}", request.TicketId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
