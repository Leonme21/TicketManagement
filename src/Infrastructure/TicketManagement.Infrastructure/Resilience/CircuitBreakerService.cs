using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace TicketManagement.Infrastructure.Resilience;

/// <summary>
/// ✅ NEW: Circuit breaker service for external service calls
/// Prevents cascading failures by breaking the circuit when errors exceed threshold
/// </summary>
public interface ICircuitBreakerService
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, string serviceName);
}

public sealed class CircuitBreakerService : ICircuitBreakerService
{
    private readonly ILogger<CircuitBreakerService> _logger;
    private readonly Dictionary<string, AsyncCircuitBreakerPolicy> _policies = new();

    public CircuitBreakerService(ILogger<CircuitBreakerService> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string serviceName)
    {
        var policy = GetOrCreatePolicy(serviceName);
        return await policy.ExecuteAsync(action);
    }

    private AsyncCircuitBreakerPolicy GetOrCreatePolicy(string serviceName)
    {
        if (_policies.TryGetValue(serviceName, out var existingPolicy))
            return existingPolicy;

        var policy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogWarning(
                        "Circuit breaker opened for {ServiceName} due to {ExceptionType}. Breaking for {Duration}s",
                        serviceName,
                        exception.GetType().Name,
                        duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation(
                        "Circuit breaker reset for {ServiceName}",
                        serviceName);
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation(
                        "Circuit breaker half-open for {ServiceName}, testing service",
                        serviceName);
                });

        _policies[serviceName] = policy;
        return policy;
    }
}
