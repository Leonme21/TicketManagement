namespace TicketManagement.Application.Common.Interfaces;

public record RateLimitResult
{
    public bool IsAllowed { get; init; }
    public int RemainingRequests { get; init; }
    public int Limit { get; init; }
    public TimeSpan? RetryAfter { get; init; }
    public DateTime? ResetTime { get; init; }
}
