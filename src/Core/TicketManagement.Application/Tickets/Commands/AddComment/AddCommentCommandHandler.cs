using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.AddComment;

/// <summary>
/// 🔥 BIG TECH LEVEL: Handler for adding comments to tickets
/// Uses Repository for aggregate operations, DbContext for SaveChanges
/// </summary>
public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<int>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<AddCommentCommandHandler> _logger;

    public AddCommentCommandHandler(
        ITicketRepository ticketRepository,
        ICurrentUserService currentUserService,
        IApplicationDbContext dbContext,
        ILogger<AddCommentCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized comment attempt on ticket {TicketId}", request.TicketId);
            return Result.Unauthorized<int>("User not authenticated");
        }

        // Get ticket aggregate
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for comment", request.TicketId);
            return Result.NotFound<int>("Ticket", request.TicketId);
        }

        // 🔥 Add comment through domain (domain event emitted)
        var commentResult = ticket.AddComment(request.Content, userId, request.IsInternal);
        if (commentResult.IsFailure)
        {
            _logger.LogWarning("Failed to add comment to ticket {TicketId}: {Error}", 
                request.TicketId, commentResult.Error);
            return Result.Invalid<int>(commentResult.Error.Description);
        }

        try
        {
            _ticketRepository.Update(ticket);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Comment added to ticket {TicketId} by user {UserId}",
                request.TicketId, userId);

            return Result.Success(ticket.Comments.Last().Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save comment for ticket {TicketId}", request.TicketId);
            return Result.InternalError<int>("Failed to save comment due to database error");
        }
    }
}
