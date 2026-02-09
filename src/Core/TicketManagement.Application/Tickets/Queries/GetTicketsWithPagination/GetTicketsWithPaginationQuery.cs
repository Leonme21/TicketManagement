using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsWithPagination;

/// <summary>
/// 🔥 BIG TECH LEVEL: Optimized query with filtering and pagination
/// Returns Result pattern for consistent error handling
/// </summary>
public sealed record GetTicketsWithPaginationQuery : IRequest<Result<PaginatedResult<TicketSummaryDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    
    // Filtering
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public int? CategoryId { get; init; }
    public int? AssignedToId { get; init; }
    public int? CreatorId { get; init; }
    public string? SearchTerm { get; init; }
    
    // Date filtering
    public DateTimeOffset? CreatedAfter { get; init; }
    public DateTimeOffset? CreatedBefore { get; init; }
    
    // Sorting
    public string? SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}