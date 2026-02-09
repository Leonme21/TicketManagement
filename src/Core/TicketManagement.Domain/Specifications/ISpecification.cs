using System.Linq.Expressions;

namespace TicketManagement.Domain.Specifications;

/// <summary>
/// 🔥 STAFF LEVEL: Specification Pattern for flexible query composition
/// Allows building complex queries without hardcoding Includes in repositories
/// Based on Eric Evans' DDD and Martin Fowler's Specification Pattern
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Criteria expression for filtering (WHERE clause)
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }
    
    /// <summary>
    /// Include expressions for eager loading (JOIN)
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// Include strings for nested properties (e.g., "Creator.Department")
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Order by expression (ascending)
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }
    
    /// <summary>
    /// Order by expression (descending)
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    /// <summary>
    /// Enable split query for multiple collections (prevents cartesian explosion)
    /// </summary>
    bool IsSplitQuery { get; }
    
    /// <summary>
    /// Enable tracking for write operations
    /// </summary>
    bool IsTrackingEnabled { get; }
}
