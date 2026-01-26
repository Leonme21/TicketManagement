using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Common;

public class TicketFilter
{
    public int? CategoryId { get; set; }
    public TicketPriority? Priority { get; set; }
    public TicketStatus? Status { get; set; }
    public int? CreatorId { get; set; }
    public int? AssignedToId { get; set; }
}
