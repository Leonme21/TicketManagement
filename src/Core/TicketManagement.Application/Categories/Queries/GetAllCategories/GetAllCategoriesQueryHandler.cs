using System.Diagnostics;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Categories;

namespace TicketManagement.Application.Categories.Queries.GetAllCategories;

/// <summary>
/// ? STAFF LEVEL: Query optimizado con caching para categorías (datos casi estáticos)
/// TTL largo porque las categorías rara vez cambian
/// </summary>
public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly ILogger<GetAllCategoriesQueryHandler> _logger;
    private static readonly ActivitySource ActivitySource = new("TicketManagement.Queries");

    public GetAllCategoriesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICacheService cache,
        ILogger<GetAllCategoriesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("GetAllCategories");

        try
        {
            // ? 1. Try cache first (1 hour TTL for categories - they rarely change)
            var cacheKey = CacheKeys.AllCategories();
            var cachedCategories = await _cache.GetAsync<List<CategoryDto>>(cacheKey, cancellationToken);

            if (cachedCategories is not null)
            {
                _logger.LogInformation("Cache HIT for all categories ({Count} items)", cachedCategories.Count);
                activity?.SetTag("cache.hit", true);
                activity?.SetTag("categories.count", cachedCategories.Count);
                return cachedCategories;
            }

            _logger.LogDebug("Cache MISS for categories, querying database");
            activity?.SetTag("cache.hit", false);

            // ? 2. Query database with projection
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            // ? 3. Store in cache (1 hour TTL - categories are almost static)
            await _cache.SetAsync(
                cacheKey,
                dtos,
                TimeSpan.FromHours(1),
                cancellationToken);

            _logger.LogInformation(
                "Categories retrieved from DB and cached ({Count} items) for {CacheDuration} hour",
                dtos.Count,
                1);

            activity?.SetTag("categories.count", dtos.Count);
            activity?.SetTag("result", "success");

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
