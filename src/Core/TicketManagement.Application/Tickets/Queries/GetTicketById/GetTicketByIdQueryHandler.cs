using System.Diagnostics;
using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tickets.Queries.GetTicketById;

/// <summary>
/// ?? BIG TECH LEVEL: Handler optimized with caching, metrics and distributed tracing
/// Uses ITicketQueryService for read operations (CQRS Read Side)
/// Cache-Aside Pattern + OpenTelemetry + Structured Logging
/// </summary>
public sealed class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, Result<TicketDetailsDto>>
{
    private readonly ITicketQueryService _queryService;
    private readonly ICacheService _cache;
    private readonly ILogger<GetTicketByIdQueryHandler> _logger;
    private static readonly ActivitySource ActivitySource = new("TicketManagement.Queries");

    public GetTicketByIdQueryHandler(
        ITicketQueryService queryService,
        ICacheService cache,
        ILogger<GetTicketByIdQueryHandler> logger)
    {
        _queryService = queryService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<TicketDetailsDto>> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("GetTicketById");
        activity?.SetTag("ticket.id", request.TicketId);

        try
        {
            // ?? 1. Try cache first (Cache-Aside Pattern)
            var cacheKey = CacheKeys.TicketDetails(request.TicketId);
            var cachedDto = await _cache.GetAsync<TicketDetailsDto>(cacheKey, cancellationToken);

            if (cachedDto is not null)
            {
                _logger.LogInformation("Cache HIT for Ticket {TicketId}", request.TicketId);
                activity?.SetTag("cache.hit", true);
                return Result.Success(cachedDto);
            }

            _logger.LogDebug("Cache MISS for Ticket {TicketId}, querying database", request.TicketId);
            activity?.SetTag("cache.hit", false);

            // ?? 2. Query using QueryService (CQRS Read Side - optimized projections)
            var dto = await _queryService.GetTicketDetailsAsync(request.TicketId, cancellationToken);

            if (dto is null)
            {
                _logger.LogWarning("Ticket {TicketId} not found", request.TicketId);
                activity?.SetTag("result", "not_found");
                return Result.NotFound<TicketDetailsDto>("Ticket", request.TicketId);
            }

            // ?? 3. Store in cache (5 minutes TTL for ticket details)
            await _cache.SetAsync(
                cacheKey, 
                dto, 
                TimeSpan.FromMinutes(5), 
                cancellationToken);

            _logger.LogInformation(
                "Ticket {TicketId} retrieved from DB and cached for {CacheDuration} minutes", 
                request.TicketId,
                5);

            activity?.SetTag("result", "success");
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Ticket {TicketId}", request.TicketId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
