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
/// Handler que procesa el login
/// Delega autenticación a IIdentityService (implementado en Infrastructure)
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return result;
    }
}
