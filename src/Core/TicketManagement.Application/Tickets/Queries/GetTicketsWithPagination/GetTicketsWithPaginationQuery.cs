using MediatR;
using TicketManagement.Application.Contracts.Common;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsWithPagination;

/// <summary>
/// Query para obtener tickets con paginación
/// Usa enums fuertemente tipados para evitar errores de parsing
/// </summary>
public record GetTicketsWithPaginationQuery : IRequest<PaginatedList<TicketDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public int? CategoryId { get; init; }
}
