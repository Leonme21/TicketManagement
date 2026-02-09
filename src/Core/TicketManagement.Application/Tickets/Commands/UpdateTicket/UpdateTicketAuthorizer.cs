using System.Threading;
using System.Threading.Tasks;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.UpdateTicket;

public class UpdateTicketAuthorizer : IAuthorizer<UpdateTicketCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITicketAuthorizationService _ticketAuthorizationService;

    public UpdateTicketAuthorizer(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITicketAuthorizationService ticketAuthorizationService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _ticketAuthorizationService = ticketAuthorizationService;
    }

    public async Task<Result> AuthorizeAsync(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.NotFound($"Ticket {request.TicketId} not found");
        }

        var userId = _currentUserService.GetUserId();
        var canModify = await _ticketAuthorizationService.CanModifyTicketAsync(userId, ticket, cancellationToken);

        if (!canModify)
        {
            return Result.Forbidden("You can only update your own tickets, or be an Admin");
        }

        return Result.Success();
    }
}
