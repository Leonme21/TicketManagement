using TicketManagement.Application.Common.Security;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using MediatR;

namespace TicketManagement.Application.Tickets.Commands.CloseTicket;

[Authorize(Policy = "CanCloseTickets")]
public record CloseTicketCommand : IRequest<Result>
{
    public int TicketId { get; init; }
    public string? Reason { get; init; }
    public string? Resolution { get; init; }
}
