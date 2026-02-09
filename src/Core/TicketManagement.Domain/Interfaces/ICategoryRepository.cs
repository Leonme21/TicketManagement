using TicketManagement.Domain.Entities;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para Categories
/// ? REFACTORIZADO: Eliminado SaveChangesAsync - responsabilidad de UnitOfWork
/// </summary>
public interface ICategoryRepository
{
    // Operaciones b�sicas
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    // ❌ REMOVED: GetAllAsync - Use specific queries with pagination instead
    void Add(Category category);
    void Update(Category category);
    void Delete(Category category);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    // Queries espec�ficas
    Task<Category?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default);
}
