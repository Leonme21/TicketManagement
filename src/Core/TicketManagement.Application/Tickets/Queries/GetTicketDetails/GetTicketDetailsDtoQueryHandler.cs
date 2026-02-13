using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Common;
using System.Diagnostics;

namespace TicketManagement.Application.Tickets.Queries.GetTicketDetails;

/// <summary>
///  Interface for specialized ticket DTO queries
/// </summary>
public interface IGetTicketDetailsDtoQuery
{
    Task<Result<TicketDetailsDto>> GetTicketDetailsAsync(int ticketId, CancellationToken ct = default);
    Task<List<TicketDto>> GetTicketListDtoAsync(int pageNumber, int pageSize, CancellationToken ct = default);
}

/// <summary>
///  Service implementation para consultas de tickets
/// Implementa lgica de lectura optimizada con proyecciones y cache
/// </summary>
public class GetTicketDetailsDtoQueryHandler : IGetTicketDetailsDtoQuery
{
    private readonly IApplicationDbContext _context;

    public GetTicketDetailsDtoQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TicketDetailsDto>> GetTicketDetailsAsync(int ticketId, CancellationToken ct = default)
    {
        var ticketDto = await QueryDatabaseAsync(ticketId, ct);

        if (ticketDto == null)
            return Result<TicketDetailsDto>.NotFound($"Ticket {ticketId} not found");

        return Result<TicketDetailsDto>.Success(ticketDto);
    }

    /// <summary>
    ///  Optimized database query with explicit projections
    /// </summary>
    private async Task<TicketDetailsDto?> QueryDatabaseAsync(int ticketId, CancellationToken ct)
    {
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
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,

                CreatorId = t.CreatorId,
                CreatorName = t.Creator != null ? t.Creator.FirstName + " " + t.Creator.LastName : "Unknown",
                CreatorEmail = t.Creator != null && t.Creator.Email != null ? t.Creator.Email.Value : "Unknown",

                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : "Unknown",

                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo != null
                    ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName
                    : null,

                Comments = t.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(20)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        AuthorId = c.AuthorId,
                        AuthorName = c.Author != null ? c.Author.FirstName + " " + c.Author.LastName : "Unknown",
                        CreatedAt = c.CreatedAt,
                        IsInternal = c.IsInternal
                    })
                    .ToList(),

                Attachments = t.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.OriginalFileName,
                    Size = a.FileSizeBytes,
                    ContentType = a.ContentType,
                    UploadedBy = "Unknown", // Temporarily mapped
                    DownloadUrl = $"/api/attachments/{a.Id}/download",
                    UploadedAt = DateTimeOffset.UtcNow // Missing on Entity?
                }).ToList(),
                
                Tags = new List<TagDto>(), // Tags navigation property missing on Entity
                RowVersion = t.RowVersion
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<List<TicketDto>> GetTicketListDtoAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                Title = t.Title.Value,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                CreatorName = t.Creator != null ? t.Creator.FirstName + " " + t.Creator.LastName : "Unknown",
                CategoryName = t.Category != null ? t.Category.Name : "Unknown",
                AssignedToName = t.AssignedTo != null
                    ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName
                    : null
            })
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
