namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// 🔥 PRODUCTION-READY: Rate limiting service interface
/// Provides flexible rate limiting for different operations and users
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Checks if the operation is allowed for the user
    /// </summary>
    Task<RateLimitResult> CheckLimitAsync(int userId, string operation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Records an operation for rate limiting tracking
    /// </summary>
    Task RecordOperationAsync(int userId, string operation, CancellationToken cancellationToken = default);
    
}