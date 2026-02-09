using Microsoft.EntityFrameworkCore;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository para Categories
/// </summary>
public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public void Delete(Category category)
    {
        Remove(category);
    }

    /// <summary>
    /// Obtiene categoría por nombre
    /// </summary>
    public async Task<Category?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, ct);
    }

    /// <summary>
    /// Obtiene solo categorías activas
    /// </summary>
    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted) // Soft delete filter
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }
}
