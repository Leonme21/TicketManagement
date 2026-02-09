using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// ✅ NEW: Logging behavior for all MediatR requests
/// Logs request execution time, success/failure, and user context
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Executing {RequestName} for User {UserId}",
            requestName,
            userId);

        try
        {
            var response = await next();
            
            stopwatch.Stop();

            _logger.LogInformation(
                "Completed {RequestName} for User {UserId} in {ElapsedMs}ms",
                requestName,
                userId,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Failed {RequestName} for User {UserId} after {ElapsedMs}ms - Error: {ErrorMessage}",
                requestName,
                userId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
