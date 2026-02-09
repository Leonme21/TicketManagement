using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Interfaces;

public interface ITicketAuthorizationService
{
    Task<bool> CanAddCommentAsync(int userId, Ticket ticket, CancellationToken cancellationToken);
    Task<bool> CanModifyTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken);
}
