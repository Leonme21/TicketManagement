using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags"); // Nombre de la tabla
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50); // Validación de BD
            builder.Property(t => t.Color)
                .HasMaxLength(7); // #RRGGBB
        }
    }
}
