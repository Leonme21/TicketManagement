using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuraci�n de la tabla Users en MySQL
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        // ✅ REFACTORED: Email as Value Object with Value Converter
        // Maps Email Value Object to string column in database
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,                    // To database: Email -> string
                value => TicketManagement.Domain.ValueObjects.Email.Create(value).Value!) // From database: string -> Email
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(254); // RFC 5321 maximum length

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>() // Guarda enum como string en BD
            .HasMaxLength(20);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Soft Delete
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(u => u.IsDeleted);

        builder.Property(u => u.DeletedAt);
        builder.Property(u => u.DeletedBy).HasMaxLength(100);

        // Auditor�a
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(100);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100);

        // Relaciones (EF Core las configura autom�ticamente, pero las hacemos expl�citas)
        builder.HasMany(u => u.CreatedTickets)
            .WithOne(t => t.Creator)
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict); // No eliminar user si tiene tickets

        builder.HasMany(u => u.AssignedTickets)
            .WithOne(t => t.AssignedTo)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull); // Si se elimina agente, ticket queda sin asignar

        builder.HasMany(u => u.Comments)
            .WithOne(c => c.Author)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query Filter (global) - excluye registros eliminados
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
