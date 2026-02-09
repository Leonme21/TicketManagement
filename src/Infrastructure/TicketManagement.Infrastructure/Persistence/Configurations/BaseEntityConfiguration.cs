using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Common;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración base para todas las entidades del dominio
/// ? Elimina duplicación de código en configuraciones
/// ? Aplica configuración común de auditoría, concurrencia y soft delete
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);
        
        // ==================== AUDITORÍA ====================
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.UpdatedAt);
        
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);
        
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);
        
        // ==================== CONCURRENCIA OPTIMISTA ====================
        builder.Property(e => e.RowVersion)
            .IsRowVersion();
        
        // ==================== SOFT DELETE (si aplica) ====================
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.HasIndex("IsDeleted");
            
            builder.Property("DeletedAt");
            
            builder.Property("DeletedBy")
                .HasMaxLength(100);
            
            // Query Filter aplicado en ApplicationDbContext.OnModelCreating
        }
    }
}
