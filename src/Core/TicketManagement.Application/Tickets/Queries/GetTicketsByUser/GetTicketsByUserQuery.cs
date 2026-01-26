using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Contracts.Common;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsByUser;

/// <summary>
/// Query para obtener tickets creados por el usuario autenticado
/// </summary>
public record GetTicketsByUserQuery : IRequest<PaginatedList<TicketDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
