using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsByAgent;

/// <summary>
/// ?? BIG TECH LEVEL: Handler using ITicketQueryService for read operations (CQRS)
/// </summary>
public sealed class GetTicketsByAgentQueryHandler : IRequestHandler<GetTicketsByAgentQuery, Result<PaginatedResult<TicketSummaryDto>>>
{
    private readonly ITicketQueryService _queryService;
    private readonly ICurrentUserService _currentUserService;

    public GetTicketsByAgentQueryHandler(
        ITicketQueryService queryService,
        ICurrentUserService currentUserService)
    {
        _queryService = queryService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<TicketSummaryDto>>> Handle(GetTicketsByAgentQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == 0)
        {
            return Result.Unauthorized<PaginatedResult<TicketSummaryDto>>("User is not authenticated");
        }

        var filter = new TicketQueryFilter
        {
            AssignedToId = userId,
            Status = request.Status,
            Priority = request.Priority,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        var result = await _queryService.GetPaginatedAsync(
            filter,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(result);
    }
}

/// <summary>
/// Query for getting agent's assigned tickets with pagination
/// </summary>
public record GetTicketsByAgentQuery : IRequest<Result<PaginatedResult<TicketSummaryDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}
