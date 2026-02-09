using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Application.Contracts.Tickets;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ? BIG TECH LEVEL: Statistical queries for tickets (SRP compliant)
/// Used for dashboards, rate limiting validation, and business rules
/// </summary>
public interface ITicketStatisticsService
{
    /// <summary>
    /// Gets the count of tickets created by a user on a specific date
    /// </summary>
    Task<int> GetUserTicketCountForDateAsync(int userId, DateTime date, CancellationToken ct = default);

    /// <summary>
    /// Gets the count of critical tickets created by a user on a specific date
    /// </summary>
    Task<int> GetUserCriticalTicketCountForDateAsync(int userId, DateTime date, CancellationToken ct = default);

    /// <summary>
    /// Gets the count of active (non-closed/resolved) tickets in the system
    /// </summary>
    Task<int> GetActiveTicketCountAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the count of tickets created by a user today
    /// </summary>
    Task<int> CountUserTicketsTodayAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user can create a ticket (business rule validation)
    /// </summary>
    Task<bool> CanUserCreateTicketAsync(int userId, CancellationToken ct = default);
}

/// <summary>
/// ? BIG TECH LEVEL: List queries for tickets (SRP compliant)
/// Used for ticket listings without pagination
/// </summary>
public interface ITicketListQueryService
{
    /// <summary>
    /// Gets recent tickets created by a user since a specific time
    /// </summary>
    Task<IReadOnlyList<TicketSummaryDto>> GetUserRecentTicketsAsync(int userId, DateTimeOffset since, CancellationToken ct = default);

    /// <summary>
    /// Gets tickets that have exceeded their SLA deadline
    /// </summary>
    Task<IReadOnlyList<TicketSummaryDto>> GetOverdueTicketsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets unassigned tickets (limited for dashboard/queue display)
    /// </summary>
    Task<IReadOnlyList<TicketSummaryDto>> GetUnassignedTicketsAsync(int limit = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets tickets created by a specific user
    /// </summary>
    Task<IReadOnlyList<TicketSummaryDto>> GetTicketsByCreatorAsync(int creatorId, CancellationToken ct = default);

    /// <summary>
    /// Gets tickets assigned to a specific agent
    /// </summary>
    Task<IReadOnlyList<TicketSummaryDto>> GetTicketsByAgentAsync(int agentId, CancellationToken ct = default);
}

/// <summary>
/// ? BIG TECH LEVEL: Paginated query for tickets (SRP compliant)
/// Used for main ticket listings with filtering and pagination
/// </summary>
public interface ITicketPaginatedQueryService
{
    /// <summary>
    /// Gets paginated tickets with optional filtering
    /// </summary>
    Task<PaginatedResult<TicketSummaryDto>> GetPaginatedAsync(
        TicketQueryFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}

/// <summary>
/// ? BIG TECH LEVEL: Single ticket detail queries (SRP compliant)
/// Used for ticket detail views
/// </summary>
public interface ITicketDetailsQueryService
{
    /// <summary>
    /// Gets detailed ticket information for display (read model)
    /// </summary>
    Task<TicketDetailsDto?> GetTicketDetailsAsync(int ticketId, CancellationToken ct = default);
}

/// <summary>
/// ? BACKWARD COMPATIBLE: Composite interface for existing code
/// Combines all query interfaces for backward compatibility
/// New code should depend on specific interfaces (ISP principle)
/// </summary>
public interface ITicketQueryService : 
    ITicketStatisticsService, 
    ITicketListQueryService, 
    ITicketPaginatedQueryService,
    ITicketDetailsQueryService
{
}

/// <summary>
/// Query filter for paginated ticket queries
/// </summary>
public record TicketQueryFilter
{
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public int? CategoryId { get; init; }
    public int? AssignedToId { get; init; }
    public int? CreatorId { get; init; }
    public string? SearchTerm { get; init; }
    public DateTimeOffset? CreatedAfter { get; init; }
    public DateTimeOffset? CreatedBefore { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}
