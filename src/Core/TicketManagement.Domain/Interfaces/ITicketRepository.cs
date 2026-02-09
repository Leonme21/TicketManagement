using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Specifications;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// 🔥 BIG TECH LEVEL: Lean repository interface with Specification Pattern support
/// Domain aggregate operations only - Query-specific methods belong in Application Layer (CQRS Read Side)
/// Following DDD principles: Repository handles aggregate persistence only
/// ✅ REFACTORED: Added Specification Pattern for flexible queries
/// </summary>
public interface ITicketRepository
{
    // === CORE AGGREGATE OPERATIONS (WRITE SIDE) ===
    
    /// <summary>
    /// Gets a ticket by ID with tracking enabled for modifications
    /// </summary>
    Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// ✅ NEW: Gets a ticket using a Specification (flexible includes)
    /// Replaces hardcoded GetByIdWithDetailsAsync
    /// </summary>
    Task<Ticket?> GetBySpecificationAsync(ISpecification<Ticket> specification, CancellationToken ct = default);

    /// <summary>
    /// Adds a new ticket to the repository
    /// </summary>
    Task AddAsync(Ticket ticket, CancellationToken ct = default);

    /// <summary>
    /// Marks a ticket as modified
    /// </summary>
    void Update(Ticket ticket);

    /// <summary>
    /// Removes a ticket from the repository
    /// </summary>
    void Remove(Ticket ticket);

    /// <summary>
    /// Checks if a ticket exists by ID
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
