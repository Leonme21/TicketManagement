using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsWithPagination;

/// <summary>
/// 🔥 BIG TECH LEVEL: Clean query handler using ITicketQueryService (CQRS Read Side)
/// Direct query service access with optimized projections
/// </summary>
public sealed class GetTicketsWithPaginationQueryHandler 
    : IRequestHandler<GetTicketsWithPaginationQuery, Result<PaginatedResult<TicketSummaryDto>>>
{
    private readonly ITicketQueryService _queryService;

    public GetTicketsWithPaginationQueryHandler(ITicketQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<PaginatedResult<TicketSummaryDto>>> Handle(
        GetTicketsWithPaginationQuery request, 
        CancellationToken cancellationToken)
    {
        // Convert query to filter object
        var filter = new TicketQueryFilter
        {
            Status = request.Status,
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            AssignedToId = request.AssignedToId,
            CreatorId = request.CreatorId,
            SearchTerm = request.SearchTerm,
            CreatedAfter = request.CreatedAfter,
            CreatedBefore = request.CreatedBefore,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        // Use QueryService for optimized read operations
        var result = await _queryService.GetPaginatedAsync(
            filter, 
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        return Result.Success(result);
    }
}