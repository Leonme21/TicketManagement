namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ?? BIG TECH LEVEL: Cache warmup service interface
/// Used to pre-populate cache with frequently accessed data
/// </summary>
public interface ICacheWarmupService
{
    /// <summary>
    /// Warms up cache with popular/frequently accessed tickets
    /// </summary>
    Task WarmupPopularTicketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Warms up cache with all categories
    /// </summary>
    Task WarmupCategoriesAsync(CancellationToken cancellationToken = default);
}
