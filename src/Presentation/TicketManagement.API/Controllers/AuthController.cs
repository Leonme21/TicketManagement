using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Authentication.Commands.Login;
using TicketManagement.Application.Authentication.Commands.Register;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// Endpoints de autenticación (Login/Register)
/// </summary>
public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    /// <response code="200">Usuario registrado exitosamente</response>
    /// <response code="400">Validación fallida o email duplicado</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Autentica un usuario existente
    /// </summary>
    /// <response code="200">Login exitoso, retorna JWT token</response>
    /// <response code="400">Credenciales inválidas</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }
}
