using Microsoft.EntityFrameworkCore;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// ✅ BIG TECH LEVEL: Generic Repository Implementation
/// Provides standard CRUD operations for entities
/// REFACTORED: Removed GetAllAsync to prevent OOM exceptions in production
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    // ❌ REMOVED: GetAllAsync() - Dangerous method that can cause OOM
    // Use pagination or specific queries with filters instead

    public virtual void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public virtual void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Set<T>().AnyAsync(e => e.Id == id, ct);
    }
    
    public void Remove(T entity)
    {
         _context.Set<T>().Remove(entity);
    }
}
