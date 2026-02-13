using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Infrastructure.Persistence;

namespace TicketManagement.Infrastructure.Queries;

/// <summary>
/// ✅ BIG TECH LEVEL: Query service implementation for read operations (CQRS Read Side)
/// Implements ISP-compliant interfaces: ITicketStatisticsService, ITicketListQueryService, etc.
/// All queries use AsNoTracking for optimal performance
/// Projections are done at database level for efficiency
/// </summary>
public sealed class TicketQueryService : ITicketQueryService
{
    private readonly ApplicationDbContext _context;

    public TicketQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ✅ BIG TECH LEVEL: Reusable Expression Tree for efficient projection across all queries.
    /// EF Core translates this to optimized SQL SELECT statements.
    /// </summary>
    private static readonly System.Linq.Expressions.Expression<Func<Domain.Entities.Ticket, TicketSummaryDto>> SummaryProjection = 
        t => new TicketSummaryDto
        {
            Id = t.Id,
            Title = t.Title.Value,
            Status = t.Status,
            Priority = t.Priority,
            CategoryName = t.Category != null ? t.Category.Name : "Unknown",
            AssignedToName = t.AssignedTo != null ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName : null,
            CreatorName = t.Creator != null ? t.Creator.FirstName + " " + t.Creator.LastName : "Unknown",
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CommentCount = t.Comments.Count(),
            IsOverdue = false // Calculated client-side or in separate pass
        };

    #region ITicketStatisticsService Implementation

    public async Task<int> GetUserTicketCountForDateAsync(int userId, DateTime date, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.CreatorId == userId && t.CreatedAt.Date == date.Date, ct);
    }

    public async Task<int> GetUserCriticalTicketCountForDateAsync(int userId, DateTime date, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.CreatorId == userId && 
                           t.CreatedAt.Date == date.Date && 
                           t.Priority == TicketPriority.Critical, ct);
    }

    public async Task<int> GetActiveTicketCountAsync(CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.Status != TicketStatus.Closed && 
                           t.Status != TicketStatus.Resolved, ct);
    }

    public async Task<int> CountUserTicketsTodayAsync(int userId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.CreatorId == userId && t.CreatedAt.Date == today, ct);
    }

    public async Task<bool> CanUserCreateTicketAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.IsActive, ct);
    }

    #endregion

    #region ITicketListQueryService Implementation

    public async Task<IReadOnlyList<TicketSummaryDto>> GetUserRecentTicketsAsync(int userId, DateTimeOffset since, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.CreatorId == userId && t.CreatedAt >= since)
            .OrderByDescending(t => t.CreatedAt)
            .Select(SummaryProjection)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketSummaryDto>> GetOverdueTicketsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
            .Where(t => 
                (t.Priority == TicketPriority.Critical && t.CreatedAt.AddHours(2) < now) ||
                (t.Priority == TicketPriority.High && t.CreatedAt.AddHours(8) < now) ||
                (t.Priority == TicketPriority.Medium && t.CreatedAt.AddHours(24) < now) ||
                (t.Priority == TicketPriority.Low && t.CreatedAt.AddHours(72) < now))
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(SummaryProjection)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketSummaryDto>> GetUnassignedTicketsAsync(int limit = 10, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.AssignedToId == null && t.Status == TicketStatus.Open)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Take(limit)
            .Select(SummaryProjection)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketSummaryDto>> GetTicketsByCreatorAsync(int creatorId, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.CreatorId == creatorId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(SummaryProjection)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketSummaryDto>> GetTicketsByAgentAsync(int agentId, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.AssignedToId == agentId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(SummaryProjection)
            .ToListAsync(ct);
    }

    #endregion

    #region ITicketPaginatedQueryService Implementation

    public async Task<PaginatedResult<TicketSummaryDto>> GetPaginatedAsync(
        TicketQueryFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.AssignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == filter.AssignedToId.Value);

        if (filter.CreatorId.HasValue)
            query = query.Where(t => t.CreatorId == filter.CreatorId.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            // ✅ Validate search term length to prevent abuse
            if (filter.SearchTerm.Length > 100)
                throw new ArgumentException("Search term cannot exceed 100 characters", nameof(filter.SearchTerm));

            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(t => 
                t.Title.Value.ToLower().Contains(searchTerm) ||
                t.Description.Value.ToLower().Contains(searchTerm));
        }

        if (filter.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.CreatedAfter.Value);

        if (filter.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.CreatedBefore.Value);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDescending);

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // ✅ BIG TECH LEVEL: Projection done at database level - avoids N+1
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(SummaryProjection)
            .ToListAsync(ct);

        return new PaginatedResult<TicketSummaryDto>(items, totalCount, pageNumber, pageSize);
    }

    #endregion

    #region ITicketDetailsQueryService Implementation

    public async Task<TicketDetailsDto?> GetTicketDetailsAsync(int ticketId, CancellationToken ct = default)
    {
        // ✅ BIG TECH LEVEL: Single query with inline projection - no N+1
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new TicketDetailsDto
            {
                Id = t.Id,
                Title = t.Title.Value,
                Description = t.Description.Value,
                Status = t.Status,
                Priority = t.Priority,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : "Unknown",
                CreatorId = t.CreatorId,
                CreatorName = t.Creator != null ? t.Creator.FirstName + " " + t.Creator.LastName : "Unknown",
                CreatorEmail = t.Creator != null && t.Creator.Email != null ? t.Creator.Email.Value : "Unknown",
                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName : null,
                AssignedToEmail = t.AssignedTo != null && t.AssignedTo.Email != null ? t.AssignedTo.Email.Value : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                RowVersion = t.RowVersion,
                Comments = t.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        IsInternal = c.IsInternal,
                        AuthorId = c.AuthorId,
                        AuthorName = c.Author != null ? c.Author.FirstName + " " + c.Author.LastName : "Unknown",
                        CreatedAt = c.CreatedAt
                    }).ToList(),
                // ✅ FIXED: Properly project UploadedBy navigation property
                Attachments = t.Attachments
                    .Select(a => new AttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.OriginalFileName,
                        ContentType = a.ContentType,
                        Size = a.FileSizeBytes,
                        UploadedAt = a.CreatedAt,
                        UploadedBy = a.UploadedBy != null 
                            ? a.UploadedBy.FirstName + " " + a.UploadedBy.LastName 
                            : "System",
                        DownloadUrl = "/api/attachments/" + a.Id + "/download"
                    }).ToList(),
                Tags = t.Tags
                    .Select(tag => new TagDto
                    {
                        Id = tag.Id,
                        Name = tag.Name,
                        Color = tag.Color
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    #endregion

    #region Private Helpers

    private static IQueryable<Domain.Entities.Ticket> ApplySorting(
        IQueryable<Domain.Entities.Ticket> query, 
        string? sortBy, 
        bool sortDescending)
    {
        return sortBy?.ToLower() switch
        {
            "priority" => sortDescending 
                ? query.OrderByDescending(t => t.Priority) 
                : query.OrderBy(t => t.Priority),
            "status" => sortDescending 
                ? query.OrderByDescending(t => t.Status) 
                : query.OrderBy(t => t.Status),
            "title" => sortDescending 
                ? query.OrderByDescending(t => t.Title.Value) 
                : query.OrderBy(t => t.Title.Value),
            "updatedat" => sortDescending 
                ? query.OrderByDescending(t => t.UpdatedAt) 
                : query.OrderBy(t => t.UpdatedAt),
            _ => sortDescending 
                ? query.OrderByDescending(t => t.CreatedAt) 
                : query.OrderBy(t => t.CreatedAt)
        };
    }

    #endregion
}
