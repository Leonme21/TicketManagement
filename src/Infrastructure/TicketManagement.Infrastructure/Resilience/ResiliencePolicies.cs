using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace TicketManagement.Infrastructure.Resilience;

/// <summary>
///  PRODUCTION-READY: Resilience policies using Polly v8+
/// Features:
/// - Retry with exponential backoff
/// - Circuit breaker pattern
/// - Timeout policies
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    ///  Registers resilience pipelines in DI container
    /// </summary>
    public static IServiceCollection AddResiliencePolicies(this IServiceCollection services)
    {
        //  Database query pipeline (retry + circuit breaker + timeout)
        services.AddResiliencePipeline("database-query", builder =>
        {
            builder
                // 1. Timeout: Prevent hung queries
                .AddTimeout(TimeSpan.FromSeconds(30))
                
                // 2. Retry: Handle transient failures
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                })
                
                // 3. Circuit Breaker: Prevent cascade failures
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5, // Open if 50% fail
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    MinimumThroughput = 5, // Minimum requests before evaluating
                    BreakDuration = TimeSpan.FromSeconds(30)
                });
        });

        //  External API call pipeline (shorter timeout, more aggressive retry)
        services.AddResiliencePipeline("external-api", builder =>
        {
            builder
                .AddTimeout(TimeSpan.FromSeconds(10))
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromMilliseconds(500),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.7,
                    SamplingDuration = TimeSpan.FromSeconds(5),
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(15)
                });
        });

        //  Cache operations pipeline (fast fail, no retry)
        services.AddResiliencePipeline("cache", builder =>
        {
            builder
                .AddTimeout(TimeSpan.FromSeconds(5))
                // No retry for cache - graceful degradation
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.8,
                    SamplingDuration = TimeSpan.FromSeconds(5),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(10)
                });
        });

        return services;
    }
}
