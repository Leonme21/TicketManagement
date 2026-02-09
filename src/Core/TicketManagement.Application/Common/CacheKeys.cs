namespace TicketManagement.Application.Common;

/// <summary>
/// ? Centralized cache key management
/// Prevents typos and ensures consistency
/// </summary>
public static class CacheKeys
{
    // ==================== TICKETS ====================
    
    public static string TicketDetails(int ticketId) => $"ticket:details:{ticketId}";
    
    public static string TicketList(int pageNumber = 1, int pageSize = 10, string? status = null, string? priority = null) 
        => $"ticket:list:{pageNumber}:{pageSize}:{status ?? "all"}:{priority ?? "all"}";
    
    public static string UserTickets(int userId) => $"ticket:user:{userId}";
    
    public static string AgentTickets(int agentId) => $"ticket:agent:{agentId}";
    
    // ==================== CATEGORIES ====================
    
    public static string AllCategories() => "categories:all";
    
    public static string CategoryDetails(int categoryId) => $"category:details:{categoryId}";
    
    // ==================== USERS ====================
    
    public static string UserDetails(int userId) => $"user:details:{userId}";
    
    public static string UserByEmail(string email) => $"user:email:{email}";
    
    // ==================== TAGS ====================
    
    public static string AllTags() => "tags:all";
    
    public static string TagDetails(int tagId) => $"tag:details:{tagId}";
    
    // ==================== PATTERNS (for bulk invalidation) ====================
    
    public static string TicketPattern(int ticketId) => $"ticket:*{ticketId}*";
    
    public static string UserPattern(int userId) => $"*user:{userId}*";
    
    public static string CategoryPattern() => "categories:*";
}
