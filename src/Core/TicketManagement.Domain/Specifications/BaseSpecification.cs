using System.Linq.Expressions;

namespace TicketManagement.Domain.Specifications;

/// <summary>
/// 🔥 STAFF LEVEL: Base implementation of Specification Pattern
/// Provides fluent API for building complex queries
/// </summary>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public bool IsSplitQuery { get; private set; }
    public bool IsTrackingEnabled { get; private set; } = true; // Default: tracking enabled

    /// <summary>
    /// Add an include expression for eager loading
    /// </summary>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add an include string for nested properties
    /// </summary>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Set order by ascending
    /// </summary>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Set order by descending
    /// </summary>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    /// <summary>
    /// Enable split query to prevent cartesian explosion with multiple collections
    /// </summary>
    protected virtual void EnableSplitQuery()
    {
        IsSplitQuery = true;
    }

    /// <summary>
    /// Disable tracking for read-only queries (performance optimization)
    /// </summary>
    protected virtual void DisableTracking()
    {
        IsTrackingEnabled = false;
    }
}
