using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Interface para validadores de reglas de negocio específicas
/// Separa la validación de dominio de las reglas de negocio de aplicación
/// </summary>
public interface IBusinessRuleValidator<in TRequest>
{
    Task<Result> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}