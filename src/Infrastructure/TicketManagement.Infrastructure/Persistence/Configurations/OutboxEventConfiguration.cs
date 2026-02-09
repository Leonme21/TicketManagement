using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Infrastructure.Persistence.Outbox;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// 🔥 BIG TECH LEVEL: Configuration for OutboxEvent entity
/// Optimized for high-throughput event processing
/// </summary>
public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("OutboxEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.EventData)
            .IsRequired()
            .HasColumnType("TEXT"); // For large JSON payloads

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ProcessedAt)
            .IsRequired(false);

        builder.Property(e => e.Error)
            .HasMaxLength(2000);

        // ✅ BIG TECH LEVEL: Optimized indexes for event processing
        builder.HasIndex(e => new { e.Processed, e.CreatedAt })
            .HasDatabaseName("IX_OutboxEvents_Processing")
            .HasFilter("Processed = 0"); // Partial index for unprocessed events

        builder.HasIndex(e => new { e.Processed, e.RetryCount })
            .HasDatabaseName("IX_OutboxEvents_Retry")
            .HasFilter("Processed = 0 AND RetryCount < 5");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_OutboxEvents_CreatedAt");
    }
}