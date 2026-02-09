using Microsoft.EntityFrameworkCore;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Domain.Specifications;

namespace TicketManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// ✅ REFACTORED: Repository with Specification Pattern support
/// Flexible queries - "Pay strictly for what you use" principle
/// Following CQRS pattern: Write operations only, queries are in TicketQueryService
/// </summary>
public sealed class TicketRepository : BaseRepository<Ticket>, ITicketRepository
{
    public TicketRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// ✅ NEW: Get ticket using Specification Pattern (flexible includes)
    /// Replaces hardcoded GetByIdWithDetailsAsync
    /// </summary>
    public async Task<Ticket?> GetBySpecificationAsync(ISpecification<Ticket> specification, CancellationToken ct = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Apply specification to queryable
    /// </summary>
    private IQueryable<Ticket> ApplySpecification(ISpecification<Ticket> spec)
    {
        var query = _context.Tickets.AsQueryable();

        // Apply criteria (WHERE clause)
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // Apply includes (JOINs)
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply include strings (for nested properties)
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // Apply split query if needed (prevents cartesian explosion)
        if (spec.IsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        // Apply tracking
        if (!spec.IsTrackingEnabled)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    public async Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _context.Tickets.AddAsync(ticket, ct);
    }
}