using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>() // Enum → String
            .HasMaxLength(20);

        builder.HasIndex(t => t.Status);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(t => t.Priority);

        // Foreign Keys
        builder.Property(t => t.CreatorId)
            .IsRequired();

        builder.Property(t => t.AssignedToId); // Nullable

        builder.Property(t => t.CategoryId)
            .IsRequired();

        builder.HasIndex(t => t.CreatorId);
        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => t.CategoryId);

        // Soft Delete
        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(t => t.IsDeleted);

        builder.Property(t => t.DeletedAt);
        builder.Property(t => t.DeletedBy).HasMaxLength(100);

        // Auditoría
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt);
        builder.Property(t => t.CreatedBy).HasMaxLength(100);
        builder.Property(t => t.UpdatedBy).HasMaxLength(100);

        // Relaciones (ya configuradas en UserConfiguration, pero las reiteramos)
        builder.HasOne(t => t.Creator)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade); // Eliminar ticket → eliminar comentarios

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query Filter
        builder.HasQueryFilter(t => !t.IsDeleted);

        // Configuración de la relación Muchos-a-Muchos
        builder.HasMany(t => t.Tags)
            .WithMany(t => t.Tickets)
            .UsingEntity(j => j.ToTable("TicketTags")); // Nombre bonito para la tabla intermedia
    }
}