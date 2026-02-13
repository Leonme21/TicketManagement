using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// ✅ SENIOR LEVEL: Optimized EF Core configuration for Ticket entity
/// Includes proper indexing, constraints, and performance optimizations
/// </summary>
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {


        // Primary Key
        builder.HasKey(t => t.Id);

        // Value Objects
        builder.OwnsOne(t => t.Title, title =>
        {
            title.Property(t => t.Value)
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(t => t.Description, description =>
        {
            description.Property(d => d.Value)
                .HasMaxLength(5000)
                .IsRequired();
        });

        // Properties
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.CreatorId)
            .IsRequired();

        builder.Property(t => t.AssignedToId)
            .IsRequired(false);

        builder.Property(t => t.CategoryId)
            .IsRequired();

        // Audit fields
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired(false);

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(100);

        // Soft Delete
        builder.Property(t => t.IsDeleted);

        builder.Property(t => t.DeletedAt)
            .IsRequired(false);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100);

        // Concurrency
        builder.Property(t => t.RowVersion)
            .IsConcurrencyToken();

        // ✅ PERFORMANCE: Strategic indexes for common queries
        builder.HasIndex(t => t.CreatorId);

        builder.HasIndex(t => t.AssignedToId);

        builder.HasIndex(t => t.CategoryId);

        builder.HasIndex(t => t.Status);

        builder.HasIndex(t => t.Priority);

        builder.HasIndex(t => t.CreatedAt);

        // ✅ PERFORMANCE: Composite indexes for common filter combinations
        builder.HasIndex(t => new { t.Status, t.Priority, t.CreatedAt });

        builder.HasIndex(t => new { t.CategoryId, t.Status });

        builder.HasIndex(t => new { t.AssignedToId, t.Status });

        // ✅ PERFORMANCE: Index for daily ticket count queries
        builder.HasIndex(t => new { t.CreatorId, t.CreatedAt });

        // Soft delete filter (applied globally in DbContext)
        builder.HasQueryFilter(t => !t.IsDeleted);

        // Relationships
        builder.HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many with Tags
        builder.HasMany(t => t.Tags)
            .WithMany(t => t.Tickets)
            .UsingEntity("TicketTags");

        // ✅ PERFORMANCE: Ignore domain events for EF Core (they're handled separately)
        builder.Ignore(t => t.DomainEvents);
    }
}