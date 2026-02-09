using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Tickets;

using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using TicketManagement.Application.Common.Extensions;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsByUser;

/// <summary>
/// ?? BIG TECH LEVEL: Handler using ITicketQueryService for read operations (CQRS)
/// </summary>
public sealed class GetTicketsByUserQueryHandler : IRequestHandler<GetTicketsByUserQuery, Result<PaginatedResult<TicketSummaryDto>>>
{
    private readonly ITicketQueryService _queryService;
    private readonly ICurrentUserService _currentUserService;

    public GetTicketsByUserQueryHandler(
        ITicketQueryService queryService,
        ICurrentUserService currentUserService)
    {
        _queryService = queryService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<TicketSummaryDto>>> Handle(GetTicketsByUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == 0)
        {
            return Result.Unauthorized<PaginatedResult<TicketSummaryDto>>("User is not authenticated");
        }

        var filter = new TicketQueryFilter
        {
            CreatorId = userId,
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
/// Query for getting user's tickets with pagination
/// </summary>
public record GetTicketsByUserQuery : IRequest<Result<PaginatedResult<TicketSummaryDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}
