using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Authentication.Commands.RefreshToken;

/// <summary>
/// ✅ Handler para renovar Access Token usando Refresh Token
/// Implementa token rotation: el refresh token usado se invalida y se genera uno nuevo
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private static readonly System.Diagnostics.ActivitySource _activitySource = new("TicketManagement.Application");

    // ✅ Logging estructurado con LoggerMessage (high-performance)
    private static readonly Action<ILogger, string, int, Exception?> _logRefreshingToken =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(2001, "RefreshToken"),
            "Refreshing token for user {UserId} from device {DeviceInfo}");

    private static readonly Action<ILogger, int, string, Exception?> _logTokenRefreshed =
        LoggerMessage.Define<int, string>(
            LogLevel.Information,
            new EventId(2002, "TokenRefreshed"),
            "Token refreshed successfully for user {UserId}, old token: {OldTokenId}");

    private static readonly Action<ILogger, string, Exception?> _logInvalidRefreshToken =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2003, "InvalidRefreshToken"),
            "Invalid refresh token used: {TokenHash}");

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("RefreshToken");
        activity?.SetTag("refresh_token_hash", HashToken(request.RefreshToken));

        try
        {
            // 1. ✅ Buscar y validar el refresh token
            var refreshToken = await _unitOfWork.RefreshTokens
                .GetByTokenAsync(request.RefreshToken, cancellationToken)
                .ConfigureAwait(false);

            if (refreshToken == null)
            {
                _logInvalidRefreshToken(_logger, HashToken(request.RefreshToken), null);
                return Result<TokenResponse>.Invalid("Invalid refresh token");
            }

            // 2. ✅ Validar que el token sea válido para uso (Usando Result Pattern)
            var validationResult = refreshToken.ValidateForUse();
            if (validationResult.IsFailure)
            {
                _logInvalidRefreshToken(_logger, HashToken(request.RefreshToken), null);
                return Result<TokenResponse>.Invalid(validationResult.Error);
            }

            // 3. ✅ Obtener el usuario asociado
            var user = await _unitOfWork.Users
                .GetByIdAsync(refreshToken.UserId, cancellationToken)
                .ConfigureAwait(false);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User {UserId} not found or inactive", refreshToken.UserId);
                return Result<TokenResponse>.Invalid("User not found or inactive");
            }

            _logRefreshingToken(_logger, request.DeviceInfo ?? "Unknown", user.Id, null);

            // 4. ✅ Generar nuevos tokens (Access + Refresh)
            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user, cancellationToken);
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, request.DeviceInfo, cancellationToken);

            // 5. ✅ Token Rotation: Marcar el token actual como usado
            refreshToken.MarkAsUsed(newRefreshToken.Token);

            // 6. ✅ Guardar cambios en una transacción
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logTokenRefreshed(_logger, user.Id, refreshToken.Id.ToString(), null);

            // 7. ✅ Construir response
            var tokenResponse = new TokenResponse
            {
                AccessToken = newAccessToken.Token,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = newAccessToken.ExpiresAt,
                TokenType = "Bearer",
                ExpiresInSeconds = (int)(newAccessToken.ExpiresAt - DateTime.UtcNow).TotalSeconds,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role.ToString()
                }
            };

            activity?.SetTag("user.id", user.Id);
            activity?.SetTag("success", true);

            return Result<TokenResponse>.Success(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            activity?.SetTag("success", false);
            return Result<TokenResponse>.Failure("An error occurred while refreshing the token");
        }
    }

    /// <summary>
    /// ✅ Hash del token para logging seguro (no exponer tokens completos en logs)
    /// </summary>
    private static string HashToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length < 8)
            return "invalid";
        
        return $"{token[..4]}...{token[^4..]}";
    }
}
