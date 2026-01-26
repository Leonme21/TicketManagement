using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace TicketManagement.Application.Tickets.Commands.CloseTicket;

/// <summary>
/// Command para cerrar ticket
/// </summary>
public record CloseTicketCommand : IRequest<Unit>
{
    public int TicketId { get; init; }
}
