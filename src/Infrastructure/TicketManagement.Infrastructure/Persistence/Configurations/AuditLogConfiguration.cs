using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entidad AuditLog
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasColumnType("TEXT"); // Para MySQL, permite almacenar JSON grandes

        builder.Property(a => a.NewValues)
            .HasColumnType("TEXT");

        builder.Property(a => a.AdditionalInfo)
            .HasMaxLength(1000);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 máximo

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        // Índices para consultas frecuentes
        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_AuditLog_Entity");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLog_User");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");

        builder.HasIndex(a => new { a.Action, a.Timestamp })
            .HasDatabaseName("IX_AuditLog_Action_Timestamp");

        // Relación con User
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditoría (heredada de BaseEntity)
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);
        builder.Property(a => a.CreatedBy).HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);
    }
}
