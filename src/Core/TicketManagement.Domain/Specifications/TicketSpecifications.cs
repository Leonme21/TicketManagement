using TicketManagement.Domain.Entities;

namespace TicketManagement.Domain.Specifications;

/// <summary>
/// 🔥 STAFF LEVEL: Concrete Specifications for Ticket queries
/// Each specification defines exactly what data is needed (Pay for what you use)
/// </summary>
public static class TicketSpecifications
{
    /// <summary>
    /// Specification for loading ticket with minimal data (ID only)
    /// Use case: Simple existence checks or status updates
    /// </summary>
    public class TicketByIdSpec : BaseSpecification<Ticket>
    {
        public TicketByIdSpec(int ticketId) : base(t => t.Id == ticketId)
        {
            // No includes - minimal query
        }
    }

    /// <summary>
    /// Specification for loading ticket with core relationships
    /// Use case: Assignment operations, status changes
    /// Performance: ~3 JOINs (Category, Creator, AssignedTo)
    /// </summary>
    public class TicketWithCoreDetailsSpec : BaseSpecification<Ticket>
    {
        public TicketWithCoreDetailsSpec(int ticketId) : base(t => t.Id == ticketId)
        {
            AddInclude(t => t.Category);
            AddInclude(t => t.Creator);
            AddInclude(t => t.AssignedTo!);
            EnableSplitQuery();
        }
    }

    /// <summary>
    /// Specification for loading ticket with all relationships
    /// Use case: Display ticket details page, full ticket view
    /// Performance: ~6 JOINs (Category, Creator, AssignedTo, Tags, Comments, Attachments)
    /// ⚠️ Use sparingly - expensive query
    /// </summary>
    public class TicketWithFullDetailsSpec : BaseSpecification<Ticket>
    {
        public TicketWithFullDetailsSpec(int ticketId) : base(t => t.Id == ticketId)
        {
            AddInclude(t => t.Category);
            AddInclude(t => t.Creator);
            AddInclude(t => t.AssignedTo!);
            AddInclude(t => t.Tags);
            AddInclude(t => t.Comments);
            AddInclude(t => t.Attachments);
            EnableSplitQuery(); // Critical for multiple collections
        }
    }

    /// <summary>
    /// Specification for loading ticket for assignment operations
    /// Use case: Assign/Unassign ticket
    /// Performance: ~2 JOINs (Creator, AssignedTo)
    /// </summary>
    public class TicketForAssignmentSpec : BaseSpecification<Ticket>
    {
        public TicketForAssignmentSpec(int ticketId) : base(t => t.Id == ticketId)
        {
            AddInclude(t => t.Creator);
            AddInclude(t => t.AssignedTo!);
            // No Category, Tags, Comments - not needed for assignment
        }
    }

    /// <summary>
    /// Specification for loading ticket for comment operations
    /// Use case: Add comment to ticket
    /// Performance: ~1 JOIN (Creator only for authorization)
    /// </summary>
    public class TicketForCommentSpec : BaseSpecification<Ticket>
    {
        public TicketForCommentSpec(int ticketId) : base(t => t.Id == ticketId)
        {
            AddInclude(t => t.Creator);
            // No other includes - comments are added separately
        }
    }
}
