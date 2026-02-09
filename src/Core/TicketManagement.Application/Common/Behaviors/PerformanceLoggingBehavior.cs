using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// ✅ ESSENTIAL: Performance monitoring for slow operations
/// </summary>
public class PerformanceLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceLoggingBehavior<TRequest, TResponse>> _logger;
    private const int SlowRequestThresholdMs = 500;

    public PerformanceLoggingBehavior(ILogger<PerformanceLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var response = await next();
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Slow request detected: {RequestName} took {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
        }
        
        return response;
    }
}