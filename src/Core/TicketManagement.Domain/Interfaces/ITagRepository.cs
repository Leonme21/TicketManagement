using TicketManagement.Domain.Entities;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para Tags
/// ? REFACTORIZADO: Eliminado SaveChangesAsync - responsabilidad de UnitOfWork
/// </summary>
public interface ITagRepository
{
    // Operaciones b�sicas
    Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
    // ❌ REMOVED: GetAllAsync - Use specific queries with pagination instead
    void Add(Tag tag);
    void Update(Tag tag);
    void Delete(Tag tag);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    // Queries espec�ficas
    Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Tag>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
