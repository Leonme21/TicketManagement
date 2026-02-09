using TicketManagement.Domain.Common;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Extensions;

public static class TicketQueryExtensions
{
    public static IQueryable<Ticket> ApplyFilter(this IQueryable<Ticket> query, TicketFilter filter)
    {
        return query
            .WhereIf(filter.CategoryId.HasValue, t => t.CategoryId == filter.CategoryId!.Value)
            .WhereIf(filter.Priority.HasValue, t => t.Priority == filter.Priority!.Value)
            .WhereIf(filter.Status.HasValue, t => t.Status == filter.Status!.Value)
            .WhereIf(filter.CreatorId.HasValue, t => t.CreatorId == filter.CreatorId!.Value)
            .WhereIf(filter.AssignedToId.HasValue, t => t.AssignedToId == filter.AssignedToId!.Value);
    }
}
