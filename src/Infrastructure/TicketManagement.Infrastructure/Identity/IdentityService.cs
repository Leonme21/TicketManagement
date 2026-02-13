using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Infrastructure.Persistence;

namespace TicketManagement.Infrastructure.Identity;

/// <summary>
/// Servicio de autenticacin e identidad
/// Implementa IIdentityService (definido en Application)
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public IdentityService(
        ApplicationDbContext context,
        PasswordHasher passwordHasher,
        JwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    /// <summary>
    /// Autentica usuario y genera JWT token
    /// </summary>
    public async Task<Result<AuthenticationResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Buscar usuario por email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);

        if (user == null)
        {
            return Result<AuthenticationResult>.Failure("Invalid email or password.");
        }

        // Verificar contrasea
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return Result<AuthenticationResult>.Failure("Invalid email or password.");
        }

        // Verificar que el usuario est activo
        if (!user.IsActive)
        {
            return Result<AuthenticationResult>.Failure("User account is deactivated.");
        }

        // Generar JWT token
        var token = _jwtTokenGenerator.GenerateToken(user);
        var expiration = _jwtTokenGenerator.GetTokenExpiration();

        var authResult = new AuthenticationResult
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAt = expiration
        };

        return Result<AuthenticationResult>.Success(authResult);
    }

    /// <summary>
    /// Registra nuevo usuario
    /// </summary>
    public async Task<Result<AuthenticationResult>> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string confirmPassword,
        string role = "Customer",
        CancellationToken cancellationToken = default)
    {
        // Verificar si el email ya existe
        if (await EmailExistsAsync(email, cancellationToken))
        {
            return Result<AuthenticationResult>.Failure("Email is already registered.");
        }

        // Parsear rol
        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            return Result<AuthenticationResult>.Failure("Invalid role.");
        }

        // Hashear contrasea
        var passwordHash = _passwordHasher.HashPassword(password);

        // Crear usuario usando Factory Method (ahora retorna Result<User>)
        var userResult = User.Create(firstName, lastName, email, passwordHash, userRole);
        
        if (userResult.IsFailure)
        {
            return Result<AuthenticationResult>.Failure(userResult.Error);
        }

        var user = userResult.Value!;
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Generar JWT token
        var token = _jwtTokenGenerator.GenerateToken(user);
        var expiration = _jwtTokenGenerator.GetTokenExpiration();

        var authResult = new AuthenticationResult
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAt = expiration
        };

        return Result<AuthenticationResult>.Success(authResult);
    }

    /// <summary>
    /// Verifica si un email ya est registrado
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }
}
