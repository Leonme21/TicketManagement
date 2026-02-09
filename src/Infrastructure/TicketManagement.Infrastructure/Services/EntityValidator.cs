using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Infrastructure.Services;

/// <summary>
/// 🔥 BIG TECH LEVEL: Centralized entity validation service
/// Reduces code duplication in command handlers
/// </summary>
public sealed class EntityValidator : IEntityValidator
{
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITicketRepository _ticketRepository;

    public EntityValidator(
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        ITicketRepository ticketRepository)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<Result> ValidateForTicketCreationAsync(int userId, int categoryId, CancellationToken ct = default)
    {
        var userResult = await ValidateUserExistsAsync(userId, ct);
        if (userResult.IsFailure)
            return userResult;

        var categoryResult = await ValidateCategoryExistsAsync(categoryId, ct);
        if (categoryResult.IsFailure)
            return categoryResult;

        return Result.Success();
    }

    public async Task<Result> ValidateUserExistsAsync(int userId, CancellationToken ct = default)
    {
        var exists = await _userRepository.ExistsAsync(userId, ct);
        return exists 
            ? Result.Success() 
            : Result.NotFound("User", userId);
    }

    public async Task<Result> ValidateCategoryExistsAsync(int categoryId, CancellationToken ct = default)
    {
        var exists = await _categoryRepository.ExistsAsync(categoryId, ct);
        return exists 
            ? Result.Success() 
            : Result.NotFound("Category", categoryId);
    }

    public async Task<Result> ValidateTicketExistsAsync(int ticketId, CancellationToken ct = default)
    {
        var exists = await _ticketRepository.ExistsAsync(ticketId, ct);
        return exists 
            ? Result.Success() 
            : Result.NotFound("Ticket", ticketId);
    }
}
