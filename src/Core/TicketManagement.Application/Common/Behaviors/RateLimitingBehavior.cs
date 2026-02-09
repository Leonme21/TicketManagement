using MediatR;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// 🔥 BIG TECH LEVEL: Pipeline behavior for rate limiting
/// Eliminates the need for complex coordinators
/// </summary>
public sealed class RateLimitingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IRateLimitService _rateLimitService;
    private readonly ICurrentUserService _currentUserService;

    public RateLimitingBehavior(IRateLimitService rateLimitService, ICurrentUserService currentUserService)
    {
        _rateLimitService = rateLimitService;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IRateLimitedRequest rateLimitedRequest)
            return await next();

        var userId = _currentUserService.GetUserId();
        var rateLimitResult = await _rateLimitService.CheckLimitAsync(userId, rateLimitedRequest.OperationType, cancellationToken);
        
        if (!rateLimitResult.IsAllowed)
        {
            throw new RateLimitExceededException(
                rateLimitedRequest.OperationType, 
                rateLimitResult.RetryAfter ?? TimeSpan.FromMinutes(1));
        }

        var response = await next();
        
        // Record successful operation
        await _rateLimitService.RecordOperationAsync(userId, rateLimitedRequest.OperationType, cancellationToken);
        
        return response;
    }
}

/// <summary>
/// Marker interface for rate-limited requests
/// </summary>
public interface IRateLimitedRequest
{
    string OperationType { get; }
}