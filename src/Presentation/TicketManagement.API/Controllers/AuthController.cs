using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TicketManagement.Application.Authentication.Commands.Login;
using TicketManagement.Application.Authentication.Commands.Register;
using TicketManagement.Application.Authentication.Commands.RefreshToken;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Contracts.Authentication;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// ? Endpoints de autenticación con Refresh Token Pattern
/// </summary>
public class AuthController : ApiControllerBase
{
    private string GetDeviceInfo()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (userAgent.Length > 500) userAgent = userAgent[..500];
        return $"{userAgent} ({ipAddress})";
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error.Description });
        return Ok(result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error.Description });
        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand 
        { 
            RefreshToken = request.RefreshToken,
            DeviceInfo = GetDeviceInfo()
        };
        
        var result = await Mediator.Send(command);
        if (result.IsSuccess) return Ok(result.Value);

        return result.Error switch
        {
            var error when error.Description.Contains("not found") || error.Description.Contains("invalid") => 
                Unauthorized(new { error = "Invalid refresh token" }),
            var error when error.Description.Contains("expired") => 
                Unauthorized(new { error = "Refresh token has expired" }),
            var error when error.Description.Contains("revoked") => 
                Unauthorized(new { error = "Refresh token has been revoked" }),
            var error when error.Description.Contains("used") => 
                Unauthorized(new { error = "Refresh token has already been used" }),
            _ => BadRequest(new { error = result.Error.Description })
        };
    }

    [HttpPost("revoke-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeAllTokens() 
    {
        await Task.CompletedTask; // ✅ Ensure async operation
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request) 
    {
        await Task.CompletedTask; // ✅ Ensure async operation
        return NoContent();
    }

    [HttpGet("devices")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveDevices()
    {
        await Task.CompletedTask; // ✅ Ensure async operation
        var devices = new[]
        {
            new { DeviceInfo = "Chrome/Windows", LastUsed = DateTime.UtcNow.AddMinutes(-5), IsCurrentDevice = true },
            new { DeviceInfo = "Mobile App/iOS", LastUsed = DateTime.UtcNow.AddHours(-2), IsCurrentDevice = false }
        };
        return Ok(devices);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        await Task.CompletedTask; // ✅ Ensure async operation
        var userInfo = new
        {
            Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            Name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        };
        return Ok(userInfo);
    }
}
