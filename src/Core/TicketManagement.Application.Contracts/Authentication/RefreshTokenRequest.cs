using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.Contracts.Authentication;

/// <summary>
/// ✅ Request DTO para renovar Access Token usando Refresh Token
/// </summary>
public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "Invalid refresh token format")]
    public required string RefreshToken { get; init; }
    
    /// <summary>
    /// Información opcional del dispositivo para auditoría
    /// </summary>
    public string? DeviceInfo { get; init; }
}
