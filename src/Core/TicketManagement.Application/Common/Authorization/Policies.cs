namespace TicketManagement.Application.Common.Authorization;

/// <summary>
/// 🔥 STAFF LEVEL: Authorization policy constants
/// Eliminates magic strings and provides compile-time safety
/// </summary>
public static class Policies
{
    /// <summary>
    /// Policy for users who can assign tickets (Agents and Admins)
    /// </summary>
    public const string CanAssignTickets = nameof(CanAssignTickets);

    /// <summary>
    /// Policy for users who can delete tickets (Admins only)
    /// </summary>
    public const string CanDeleteTickets = nameof(CanDeleteTickets);

    /// <summary>
    /// Policy for Agent or Admin roles
    /// </summary>
    public const string IsAgentOrAdmin = nameof(IsAgentOrAdmin);

    /// <summary>
    /// Policy for Admin role only
    /// </summary>
    public const string IsAdmin = nameof(IsAdmin);
}

/// <summary>
/// Role constants for consistency
/// </summary>
public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string Agent = nameof(Agent);
    public const string Customer = nameof(Customer);
}
