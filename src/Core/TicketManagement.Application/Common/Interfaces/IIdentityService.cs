using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Application.Common.Models;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Servicio de autenticación e identidad
/// Infrastructure lo implementa con JWT + BCrypt
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Autentica usuario y genera token JWT
    /// </summary>
    Task<Result<AuthenticationResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra nuevo usuario
    /// </summary>
    Task<Result<AuthenticationResult>> RegisterAsync(string firstName, string lastName, string email, string password, string confirmPassword, string role = "Customer", CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un email ya está registrado
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de autenticación exitosa
/// </summary>
public class AuthenticationResult
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}