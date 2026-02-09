using Microsoft.EntityFrameworkCore;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository para Tags
/// </summary>
public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context)
    {
    }

    public void Delete(Tag tag)
    {
        Remove(tag);
    }

    /// <summary>
    /// Obtiene tag por nombre
    /// </summary>
    public async Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Obtiene múltiples tags por sus IDs
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        return await _context.Tags
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Obtiene tags de un ticket específico
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetByTicketIdAsync(int ticketId, CancellationToken ct = default)
    {
        return await _context.Tags
            .AsNoTracking()
            .Where(t => t.Tickets.Any(ticket => ticket.Id == ticketId))
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Verifica si un nombre de tag ya existe
    /// </summary>
    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Tags
            .AsNoTracking()
            .AnyAsync(t => t.Name == name, ct);
    }
}
