namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ?? BIG TECH LEVEL: Cache invalidation service interface
/// Abstraction for cache invalidation operations
/// Implemented in Infrastructure layer
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates all cache entries for a specific ticket
    /// </summary>
    Task InvalidateTicketCacheAsync(int ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates ticket list cache (used when tickets are created/deleted)
    /// </summary>
    Task InvalidateTicketListCacheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all ticket-related cache entries for a specific user
    /// </summary>
    Task InvalidateUserTicketsCacheAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates category-related cache entries
    /// </summary>
    Task InvalidateCategoryCacheAsync(int categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cache entries (nuclear option, use sparingly)
    /// </summary>
    Task InvalidateAllCacheAsync(CancellationToken cancellationToken = default);
}
