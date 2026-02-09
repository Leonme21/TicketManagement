using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TicketManagement.Infrastructure.Services;

/// <summary>
/// ? PRODUCTION-READY: JWT token generation and validation service
/// Features:
/// - Secure access token (JWT) generation
/// - Cryptographically secure refresh token generation
/// - Comprehensive token validation
/// - Built-in logging and error handling
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _tokenValidationParameters;

    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    private static readonly Action<ILogger, int, string, Exception?> _logTokenGenerated =
        LoggerMessage.Define<int, string>(
            LogLevel.Information,
            new EventId(4001, "TokenGenerated"),
            "Access token generated for user {UserId}, expires at {ExpiresAt}");

    private static readonly Action<ILogger, int, string, Exception?> _logRefreshTokenGenerated =
        LoggerMessage.Define<int, string>(
            LogLevel.Information,
            new EventId(4002, "RefreshTokenGenerated"),
            "Refresh token generated for user {UserId}, device: {DeviceInfo}");

    public TokenService(
        IConfiguration configuration,
        IApplicationDbContext context,
        ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();

        _jwtSecret = configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _jwtIssuer = configuration["JwtSettings:Issuer"] ?? "TicketManagement";
        _jwtAudience = configuration["JwtSettings:Audience"] ?? "TicketManagement";
        _accessTokenExpiryMinutes = int.Parse(configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "15");
        _refreshTokenExpiryDays = int.Parse(configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = _jwtIssuer,
            ValidateAudience = true,
            ValidAudience = _jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public async Task<AccessTokenResult> GenerateAccessTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // ? Ensure proper async handling
        
        var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("jti", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = _tokenHandler.WriteToken(token);

        _logTokenGenerated(_logger, user.Id, expiresAt.ToString("yyyy-MM-dd HH:mm:ss"), null);

        return new AccessTokenResult
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            ExpiresInSeconds = (int)(expiresAt - DateTime.UtcNow).TotalSeconds
        };
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string? deviceInfo = null, CancellationToken cancellationToken = default)
    {
        var tokenBytes = new byte[48];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToBase64String(tokenBytes);

        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        var result = RefreshToken.Create(token, userId, expiresAt, deviceInfo);

        if (result.IsFailure)
        {
            throw new DomainException(result.Error);
        }

        var refreshToken = result.Value!;

        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logRefreshTokenGenerated(_logger, userId, deviceInfo ?? "Unknown", null);

        return refreshToken;
    }

    public async Task<TicketManagement.Application.Common.Interfaces.TokenValidationResult> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // ? Ensure proper async handling
        
        try
        {
            var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return new TicketManagement.Application.Common.Interfaces.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token algorithm"
                };
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
            var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return new TicketManagement.Application.Common.Interfaces.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid user ID in token"
                };
            }

            return new TicketManagement.Application.Common.Interfaces.TokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Email = emailClaim,
                Role = roleClaim
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new TicketManagement.Application.Common.Interfaces.TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token has expired"
            };
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return new TicketManagement.Application.Common.Interfaces.TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid token"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return new TicketManagement.Application.Common.Interfaces.TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token validation error"
            };
        }
    }

    public async Task RevokeAllUserTokensAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.Set<RefreshToken>()
            .Where(t => t.UserId == userId && t.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("All refresh tokens revoked for user {UserId}", userId);
    }
}
