using MediatR;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Contracts.Authentication;

namespace TicketManagement.Application.Authentication.Commands.RefreshToken;

/// <summary>
/// ✅ Command para renovar Access Token usando Refresh Token
/// Implementa token rotation para máxima seguridad
/// </summary>
public record RefreshTokenCommand : IRequest<Result<TokenResponse>>
{
    public required string RefreshToken { get; init; }
    public string? DeviceInfo { get; init; }
}
