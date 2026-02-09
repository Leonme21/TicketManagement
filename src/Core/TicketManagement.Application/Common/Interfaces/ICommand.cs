using MediatR;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ✅ NEW: Marker interface for commands (write operations)
/// Used by TransactionBehavior for explicit transaction detection
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// ✅ NEW: Marker interface for commands with response
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
