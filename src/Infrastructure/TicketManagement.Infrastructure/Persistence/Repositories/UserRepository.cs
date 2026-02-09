using Microsoft.EntityFrameworkCore;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository para Users con validaciones de seguridad
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public void Delete(User user)
    {
        Remove(user);
    }

    /// <summary>
    /// Busca usuario por email (para login)
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);
    }

    /// <summary>
    /// Verifica si un email ya está registrado
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email && !u.IsDeleted, ct);
    }

    /// <summary>
    /// Obtiene usuarios por rol (para asignación de tickets)
    /// </summary>
    public async Task<IReadOnlyList<User>> GetByRoleAsync(Domain.Enums.UserRole role, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == role && u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Obtiene agentes disponibles (activos y con rol Agent o Admin)
    /// </summary>
    public async Task<IReadOnlyList<User>> GetAvailableAgentsAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => 
                (u.Role == Domain.Enums.UserRole.Agent || u.Role == Domain.Enums.UserRole.Admin) &&
                u.IsActive && 
                !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Verifica si un usuario existe y está activo
    /// </summary>
    public async Task<bool> ExistsAndActiveAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == id && u.IsActive && !u.IsDeleted, ct);
    }

    /// <summary>
    /// Obtiene usuario con estadísticas de tickets
    /// </summary>
    public async Task<User?> GetByIdWithStatsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.CreatedTickets)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
    }
}
