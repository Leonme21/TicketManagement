using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Categories;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// Categories management - simplified approach
/// </summary>
[Authorize]
public class CategoriesController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public CategoriesController(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet]
    [ResponseCache(Duration = 3600)] // 1 hour - categories rarely change
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        const string cacheKey = "categories_all";
        
        if (_cache.TryGetValue(cacheKey, out List<CategoryDto>? cached))
            return cached!;

        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto 
            { 
                Id = c.Id, 
                Name = c.Name, 
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();

        _cache.Set(cacheKey, categories, TimeSpan.FromHours(1));
        return categories;
    }

    [HttpGet("{id}")]
    [ResponseCache(Duration = 1800)] // 30 minutes
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var cacheKey = $"category_{id}";
        
        if (_cache.TryGetValue(cacheKey, out CategoryDto? cached))
            return cached!;

        var category = await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto 
            { 
                Id = c.Id, 
                Name = c.Name, 
                Description = c.Description,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound();

        _cache.Set(cacheKey, category, TimeSpan.FromMinutes(30));
        return category;
    }

    [HttpPost]
    [Authorize(Policy = "IsAdmin")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        await Task.CompletedTask; // ? Ensure async operation
        // For now, return a simple response - this would need proper implementation
        return BadRequest("Category creation not implemented in simplified version");
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "IsAdmin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        await Task.CompletedTask; // ? Ensure async operation
        // For now, return a simple response - this would need proper implementation
        return BadRequest("Category update not implemented in simplified version");
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "IsAdmin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await Task.CompletedTask; // ? Ensure async operation
        // For now, return a simple response - this would need proper implementation
        return BadRequest("Category deletion not implemented in simplified version");
    }
}

// Simple DTOs
public record CreateCategoryRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record UpdateCategoryRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}