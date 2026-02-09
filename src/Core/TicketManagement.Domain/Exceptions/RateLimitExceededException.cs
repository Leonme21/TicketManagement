namespace TicketManagement.Domain.Exceptions;

/// <summary>
/// 🔥 BIG TECH LEVEL: Specific exception for rate limiting
/// </summary>
public sealed class RateLimitExceededException : DomainException
{
    public RateLimitExceededException(string operation, TimeSpan retryAfter) 
        : base($"Rate limit exceeded for operation '{operation}'. Please retry after {retryAfter.TotalMinutes:F1} minutes.")
    {
        Operation = operation;
        RetryAfter = retryAfter;
    }

    public string Operation { get; }
    public TimeSpan RetryAfter { get; }
}