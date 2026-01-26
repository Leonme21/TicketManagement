using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Common.Models;

namespace TicketManagement.Application.Authentication.Commands.Login;

/// <summary>
/// Command para autenticar usuario
/// </summary>
public record LoginCommand : IRequest<Result<AuthenticationResult>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
