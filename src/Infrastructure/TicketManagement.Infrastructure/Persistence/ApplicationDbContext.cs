using System;
using System.Collections.Generic;
using TicketManagement.Application.Common.Interfaces;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Entities;
using TicketManagement.Infrastructure.Persistence.Interceptors;
using TicketManagement.Infrastructure.Persistence.Outbox;

namespace TicketManagement.Infrastructure.Persistence;

/// <summary>
/// Principal EF Core DbContext
/// ✅ REFACTORED: Outbox logic moved to OutboxInterceptor
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;
    private readonly SoftDeleteInterceptor _softDeleteInterceptor;
    private readonly OutboxInterceptor _outboxInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor,
        SoftDeleteInterceptor softDeleteInterceptor,
        OutboxInterceptor outboxInterceptor)
        : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
        _outboxInterceptor = outboxInterceptor;
    }

    // ==================== DBSETS ====================
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    
    // Audit trail
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    // Outbox pattern
    public DbSet<Persistence.Outbox.OutboxEvent> OutboxEvents => Set<Persistence.Outbox.OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply soft delete query filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, new object[] { modelBuilder });
            }
        }

        // Basic configuration for OutboxMessage
        if (!modelBuilder.Model.GetEntityTypes().Any(e => e.ClrType == typeof(OutboxMessage)))
        {
            modelBuilder.Entity<OutboxMessage>(builder =>
            {
                builder.ToTable("OutboxMessages");
                builder.HasKey(e => e.Id);
                builder.Property(e => e.Type).IsRequired().HasMaxLength(500);
                builder.Property(e => e.Data).IsRequired();
                builder.Property(e => e.CreatedAt).IsRequired();
                builder.Property(e => e.ProcessedAt);
                builder.Property(e => e.Error).HasMaxLength(2000);
                
                // Index for processing
                builder.HasIndex(e => new { e.ProcessedAt, e.CreatedAt })
                    .HasDatabaseName("IX_OutboxMessage_Processing");
            });
        }

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// <summary>
    /// Generic method for setting soft delete filter
    /// </summary>
    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, ISoftDeletable
    {
        builder.Entity<TEntity>().HasQueryFilter(e => e.IsDeleted == false);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ✅ REFACTORED: Register all interceptors including OutboxInterceptor
        // SoftDelete must be registered BEFORE Audit to change state to Modified
        optionsBuilder.AddInterceptors(
            _softDeleteInterceptor, 
            _auditableEntityInterceptor,
            _outboxInterceptor);
    }

    /// <summary>
    /// ✅ REFACTORED: Simplified SaveChangesAsync - Outbox logic moved to interceptor
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
