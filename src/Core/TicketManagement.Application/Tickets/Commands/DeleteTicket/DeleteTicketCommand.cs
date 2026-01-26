using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace TicketManagement.Application.Tickets.Commands.DeleteTicket;

/// <summary>
/// Command para eliminar ticket (soft delete)
/// </summary>
public record DeleteTicketCommand : IRequest<Unit>
{
    public int TicketId { get; init; }
}
