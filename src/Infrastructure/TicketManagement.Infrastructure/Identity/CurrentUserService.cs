using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Identity;

/// <summary>
/// Servicio que obtiene información del usuario autenticado desde el JWT token
/// Lee los claims del HttpContext.User
/// ✅ REFACTORED: Added fallback for different claim types (sub, userId, role)
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => 
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub") ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("userId");

    public int? UserIdInt => int.TryParse(UserId, out var id) ? id : null;

    public int GetUserId()
    {
        var id = UserIdInt;
        if (!id.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return id.Value;
    }

    public string? Email => 
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");

    public string? Role => 
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("role");

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
