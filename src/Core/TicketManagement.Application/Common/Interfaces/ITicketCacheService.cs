using TicketManagement.Application.Contracts.Tickets;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ?? BIG TECH LEVEL: Intelligent caching service with invalidation strategy
/// Uses the DTOs defined in ITicketQueryService for consistency
/// </summary>
public interface ITicketCacheService
{
    /// <summary>
    /// Gets a cached ticket summary by ID
    /// </summary>
    Task<TicketSummaryDto?> GetTicketSummaryAsync(int ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cached ticket details by ID
    /// </summary>
    Task<TicketDetailsDto?> GetTicketDetailsAsync(int ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cache entries for a specific ticket
    /// </summary>
    Task InvalidateTicketCacheAsync(int ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all ticket cache entries for a specific user
    /// </summary>
    Task InvalidateUserTicketsCacheAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Warms up the cache with popular/recent tickets
    /// </summary>
    Task WarmupPopularTicketsAsync(CancellationToken cancellationToken = default);
}
