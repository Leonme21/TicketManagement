using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entidad Category
/// ? Refactorizado: Hereda de BaseEntityConfiguration para eliminar duplicación
/// </summary>
public class CategoryConfiguration : BaseEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder); // ? Aplica configuración base (auditoría, concurrencia, soft delete)

        // builder.ToTable("Categories");

        // ==================== PROPERTIES ====================
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(Category.MaxNameLength);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(Category.MaxDescriptionLength);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ==================== RELATIONSHIPS ====================
        
        builder.HasMany(c => c.Tickets)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // ? No eliminar categoría si tiene tickets asociados
    }
}
