using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// ✅ Configuración EF Core para RefreshToken
/// Optimizada para consultas de tokens y limpieza automática
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        // ✅ Token único y indexado para búsquedas rápidas
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(RefreshToken.TokenLength)
            .IsFixedLength();

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        // ✅ UserId indexado para consultas por usuario
        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        // ✅ ExpiresAt indexado para limpieza automática
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

        // ✅ IsActive indexado para consultas de tokens activos
        builder.Property(rt => rt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(rt => rt.IsActive)
            .HasDatabaseName("IX_RefreshTokens_IsActive");

        // ✅ IsUsed para token rotation
        builder.Property(rt => rt.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rt => rt.UsedAt);

        // ✅ DeviceInfo para auditoría y seguridad
        builder.Property(rt => rt.DeviceInfo)
            .HasMaxLength(500);

        // ✅ ReplacedByToken para token rotation tracking
        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(RefreshToken.TokenLength);

        // ✅ Índice compuesto para consultas optimizadas
        builder.HasIndex(rt => new { rt.UserId, rt.IsActive, rt.IsUsed, rt.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_UserActive");

        // ✅ Relación con User
        builder.HasOne(rt => rt.User)
            .WithMany() // User no necesita navegación a RefreshTokens
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Eliminar user → eliminar tokens

        // ✅ Auditoría heredada de BaseEntity
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.UpdatedAt);
        builder.Property(rt => rt.CreatedBy).HasMaxLength(100);
        builder.Property(rt => rt.UpdatedBy).HasMaxLength(100);

        // ✅ Concurrencia optimista
        builder.Property(rt => rt.RowVersion)
            .IsRowVersion();
    }
}
