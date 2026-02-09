using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para Users
/// </summary>
public interface IUserRepository
{
    // Operaciones básicas
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    void Add(User user);
    void Update(User user);
    void Delete(User user);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    // Queries específicas
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAvailableAgentsAsync(CancellationToken ct = default);
    Task<bool> ExistsAndActiveAsync(int id, CancellationToken ct = default);
    Task<User?> GetByIdWithStatsAsync(int id, CancellationToken ct = default);
}
