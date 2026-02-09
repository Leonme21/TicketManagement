using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Generic repository interface
/// ✅ REFACTORED: Removed dangerous GetAllAsync method to prevent OOM in production
/// Use pagination or specific queries instead
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    // ❌ REMOVED: Task<IReadOnlyList<T>> GetAllAsync() - Use pagination instead
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
