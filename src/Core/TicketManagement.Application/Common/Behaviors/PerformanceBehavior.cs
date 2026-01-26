﻿using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que mide tiempo de ejecución de handlers
/// Loguea warning si tarda más de 500ms (bottleneck)
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew(); // Local instance - thread-safe

        try
        {
            var response = await next();
            return response;
        }
        finally
        {
            timer.Stop();

            var elapsedMilliseconds = timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning(
                    "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)",
                    requestName,
                    elapsedMilliseconds);
            }
        }
    }
}
