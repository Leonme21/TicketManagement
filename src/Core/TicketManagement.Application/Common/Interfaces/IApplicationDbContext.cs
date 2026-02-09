using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ✅ SIMPLIFIED: Essential database context interface
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Ticket> Tickets { get; }
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}