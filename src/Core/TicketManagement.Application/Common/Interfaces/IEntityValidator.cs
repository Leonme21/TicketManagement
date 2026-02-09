using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ?? BIG TECH LEVEL: Service for validating entity existence
/// Centralizes validation logic for cleaner handlers
/// </summary>
public interface IEntityValidator
{
    /// <summary>
    /// Validates that a user and category exist before ticket creation
    /// </summary>
    Task<Result> ValidateForTicketCreationAsync(int userId, int categoryId, CancellationToken ct = default);

    /// <summary>
    /// Validates that a user exists
    /// </summary>
    Task<Result> ValidateUserExistsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Validates that a category exists
    /// </summary>
    Task<Result> ValidateCategoryExistsAsync(int categoryId, CancellationToken ct = default);

    /// <summary>
    /// Validates that a ticket exists
    /// </summary>
    Task<Result> ValidateTicketExistsAsync(int ticketId, CancellationToken ct = default);
}
