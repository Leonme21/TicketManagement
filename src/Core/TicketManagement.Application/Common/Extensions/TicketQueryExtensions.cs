using TicketManagement.Domain.Common;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Extensions;

public static class TicketQueryExtensions
{
    public static IQueryable<Ticket> ApplyFilter(this IQueryable<Ticket> query, TicketFilter filter)
    {
        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.CreatorId.HasValue)
            query = query.Where(t => t.CreatorId == filter.CreatorId.Value);

        if (filter.AssignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == filter.AssignedToId.Value);

        return query;
    }
}
