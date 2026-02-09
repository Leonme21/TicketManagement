using MediatR;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// 🔥 BIG TECH LEVEL: Pipeline behavior for authorization
/// Centralized authorization logic across all commands/queries
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationBehavior(IAuthorizationService authorizationService, ICurrentUserService currentUserService)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IAuthorizedRequest authorizedRequest)
            return await next();

        var userId = _currentUserService.GetUserId();
        var isAuthorized = await _authorizationService.HasPermissionAsync(
            userId, 
            $"{authorizedRequest.Resource}.{authorizedRequest.Action}", 
            null, 
            cancellationToken);

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException(
                $"User {userId} is not authorized to perform '{authorizedRequest.Action}' on '{authorizedRequest.Resource}'");
        }

        return await next();
    }
}

/// <summary>
/// Marker interface for authorized requests
/// </summary>
public interface IAuthorizedRequest
{
    string Resource { get; }
    string Action { get; }
}

/// <summary>
/// Marker interface for resource-specific authorization
/// </summary>
public interface IResourceAuthorizedRequest : IAuthorizedRequest
{
    int ResourceId { get; }
}