using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tickets.Queries.GetTicketById;

/// <summary>
/// ?? BIG TECH LEVEL: Query for getting ticket details
/// Returns Result pattern with TicketDetailsDto from QueryService
/// </summary>
public record GetTicketByIdQuery : IRequest<Result<TicketDetailsDto>>
{
    public int TicketId { get; init; }
}
