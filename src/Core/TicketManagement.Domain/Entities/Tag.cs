using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Domain.Entities
{
    public class Tag : BaseEntity
    {
        private Tag() { } // Para EF Core

        public Tag(string name, string color)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Ticket name cannot be empty");
            if (string.IsNullOrWhiteSpace(color))
                color = "#808080"; // Gris por defecto si no mandan color

            this.Name = name;
            this.Color = color;
        }

        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        // Relación Many-to-Many
        public ICollection<Ticket> Tickets { get; private set; } = new List<Ticket>();
    }
}
