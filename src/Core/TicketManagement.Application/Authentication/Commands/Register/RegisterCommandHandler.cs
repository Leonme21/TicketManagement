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
/// Handler que procesa el registro
/// Delega creación de usuario a IIdentityService
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.ConfirmPassword,
            "Customer",
            cancellationToken);
            

        return result;
    }
}
