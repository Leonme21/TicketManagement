using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Interface genérica para autorización basada en recursos
/// Permite validar permisos específicos por comando/query
/// </summary>
public interface IAuthorizer<in TRequest>
{
    Task<Result> AuthorizeAsync(TRequest request, CancellationToken cancellationToken = default);
}