using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Common.Exceptions;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// 🔥 STAFF LEVEL: Transaction behavior with concurrency exception handling
/// Catches infrastructure exceptions and translates them to application exceptions
/// Preserves abstraction - handlers don't need to know about EF Core
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IApplicationDbContext context,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // ✅ REFACTORED: Explicit interface check instead of naming convention
        if (request is not ICommand and not ICommand<TResponse>)
        {
            return await next();
        }

        var commandName = typeof(TRequest).Name;
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogDebug("Starting transaction for {CommandName}", commandName);
                
                var response = await next();
                
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogDebug("Transaction committed for {CommandName}", commandName);
                
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // ✅ NEW: Translate infrastructure exception to application exception
                _logger.LogWarning(ex, "Concurrency conflict in {CommandName}, rolling back", commandName);
                await transaction.RollbackAsync(cancellationToken);
                throw new ConcurrencyException($"A concurrency conflict occurred while executing {commandName}.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed for {CommandName}, rolling back", commandName);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}