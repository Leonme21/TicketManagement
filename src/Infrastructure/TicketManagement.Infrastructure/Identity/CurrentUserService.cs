using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Identity;

/// <summary>
/// Servicio que obtiene información del usuario autenticado desde el JWT token
/// Lee los claims del HttpContext.User
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public int? UserIdInt => int.TryParse(UserId, out var id) ? id : null;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
