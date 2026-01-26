using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Application.Common.Extensions;
using AutoMapper.QueryableExtensions;
using TicketManagement.Domain.Common;

namespace TicketManagement.Infrastructure.Persistence.Repositories;

public class TicketRepository : BaseRepository<Ticket>, ITicketRepository
{
    private readonly AutoMapper.IMapper _mapper;

    public TicketRepository(
        ApplicationDbContext context, 
        IDateTime dateTime, 
        AutoMapper.IMapper mapper) : base(context, dateTime) 
    {
        _mapper = mapper;
    }



    public async Task<Ticket?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        // Optimized: Only load Single-Entity relationships. 
        // Collections like Comments should be loaded via paginated separate queries.
        return await _dbSet
            .Include(t => t.Creator)
            .Include(t => t.AssignedTo)
            .Include(t => t.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<PaginatedResult<Ticket>> GetPagedAsync(
        TicketFilter filter, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.Creator)
            .Include(t => t.Category)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        // Use Extension method for cleaner code
        query = query.ApplyFilter(filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Ticket>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<T>> GetProjectedPagedAsync<T>(
        TicketFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().ApplyFilter(filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .ProjectTo<T>(_mapper.ConfigurationProvider)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
