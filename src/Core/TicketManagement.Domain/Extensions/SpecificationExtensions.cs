using TicketManagement.Domain.Specifications;

namespace TicketManagement.Domain.Extensions;

/// <summary>
/// Extension methods para usar Specification Pattern con EF Core
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Aplica un Specification a un IQueryable
    /// ? Permite usar specifications directamente en queries de EF Core
    /// </summary>
    public static IQueryable<T> Where<T>(this IQueryable<T> query, Specification<T> specification)
    {
        return query.Where(specification.ToExpression());
    }
}
