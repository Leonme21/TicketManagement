namespace TicketManagement.Infrastructure.Caching;

/// <summary>
/// Simple cache keys - focused and maintainable
/// </summary>
public static class CacheKeys
{
    private const string PREFIX = "ticketmgmt";

    // Tickets
    public static string TicketDetails(int ticketId) => $"{PREFIX}:ticket:{ticketId}";
    public static string TicketsByUser(int userId) => $"{PREFIX}:tickets:user:{userId}";
    public static string TicketsByAgent(int agentId) => $"{PREFIX}:tickets:agent:{agentId}";

    // Users
    public static string UserProfile(int userId) => $"{PREFIX}:user:{userId}";
    public static string UserByEmail(string email) => $"{PREFIX}:user:email:{email}";

    // Categories
    public static string AllCategories() => $"{PREFIX}:categories:all";
    public static string CategoryDetails(int categoryId) => $"{PREFIX}:category:{categoryId}";

    // Common TTL values
    public static class Ttl
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Long = TimeSpan.FromHours(1);
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);
    }
}
