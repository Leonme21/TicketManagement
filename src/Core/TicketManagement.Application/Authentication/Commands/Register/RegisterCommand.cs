using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Authentication.Commands.Register;

/// <summary>
/// Command para registrar nuevo usuario
/// </summary>
public record RegisterCommand : IRequest<Result<AuthenticationResult>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}
