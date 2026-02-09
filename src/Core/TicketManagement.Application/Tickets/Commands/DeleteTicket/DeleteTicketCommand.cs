using MediatR;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tickets.Commands.DeleteTicket;

/// <summary>
/// Command para eliminar ticket (soft delete)
/// </summary>
public record DeleteTicketCommand : IRequest<Result>
{
    public int TicketId { get; init; }
}
