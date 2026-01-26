using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que loguea todas las requests de MediatR
/// Incluye contexto del usuario para debugging y auditoría
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
        var userId = _currentUserService.UserId ?? "anonymous";

        _logger.LogInformation(
            "Handling {RequestName} for User {UserId}",
            requestName, userId);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "Handled {RequestName} for User {UserId}",
                requestName, userId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling {RequestName} for User {UserId}: {Message}",
                requestName, userId, ex.Message);
            throw;
        }
    }
}
