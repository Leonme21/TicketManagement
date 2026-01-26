using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Contracts.Common;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsByAgent;

/// <summary>
/// Query para obtener tickets asignados al agente autenticado
/// </summary>
public record GetTicketsByAgentQuery : IRequest<PaginatedList<TicketDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
