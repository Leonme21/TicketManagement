using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// ? BIG TECH LEVEL: Pipeline Behavior for idempotency handling
/// - Caches successful results to prevent duplicate processing
/// - Only applies to Commands that implement IIdempotentCommand
/// - Uses distributed cache for scalability
/// </summary>
public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

    public IdempotencyBehavior(
        IDistributedCache cache,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // ? Only process if request implements IIdempotentCommand
        if (request is not IIdempotentCommand idempotentCommand)
        {
            return await next();
        }

        // ? Skip if no idempotency key provided
        if (string.IsNullOrWhiteSpace(idempotentCommand.IdempotencyKey))
        {
            _logger.LogDebug(
                "No idempotency key provided for {CommandName}, skipping idempotency check",
                typeof(TRequest).Name);
            return await next();
        }

        var cacheKey = $"idempotency:{typeof(TRequest).Name}:{idempotentCommand.IdempotencyKey}";

        try
        {
            // ? Check cache for existing result
            var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogInformation(
                    "Returning cached result for idempotency key {Key} (Command: {CommandName})",
                    idempotentCommand.IdempotencyKey,
                    typeof(TRequest).Name);

                return JsonSerializer.Deserialize<TResponse>(cachedResult)!;
            }
        }
        catch (Exception ex)
        {
            // ? Cache failure should not block request processing
            _logger.LogWarning(
                ex,
                "Failed to check idempotency cache for key {Key}, proceeding with request",
                idempotentCommand.IdempotencyKey);
        }

        // ? Execute the command
        var response = await next();

        // ? Cache successful results only
        if (IsSuccessful(response))
        {
            try
            {
                var serialized = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await _cache.SetStringAsync(
                    cacheKey,
                    serialized,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                    },
                    cancellationToken);

                _logger.LogDebug(
                    "Cached successful result for idempotency key {Key} (Command: {CommandName})",
                    idempotentCommand.IdempotencyKey,
                    typeof(TRequest).Name);
            }
            catch (Exception ex)
            {
                // ? Cache failure should not fail the request
                _logger.LogWarning(
                    ex,
                    "Failed to cache result for idempotency key {Key}",
                    idempotentCommand.IdempotencyKey);
            }
        }

        return response;
    }

    /// <summary>
    /// Determines if the response was successful (for caching purposes)
    /// </summary>
    private static bool IsSuccessful(TResponse response)
    {
        return response switch
        {
            Result result => result.IsSuccess,
            null => false,
            _ => true // Non-Result responses are considered successful
        };
    }
}
