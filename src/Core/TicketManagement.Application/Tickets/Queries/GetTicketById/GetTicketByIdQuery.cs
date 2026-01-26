using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tickets;

namespace TicketManagement.Application.Tickets.Queries.GetTicketById;

/// <summary>
/// Query para obtener detalle completo de un ticket
/// </summary>
public record GetTicketByIdQuery : IRequest<TicketDetailsDto>
{
    public int TicketId { get; init; }
}
