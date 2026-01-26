using TicketManagement.Application.Contracts.Authentication;

namespace TicketManagement.BlazorWasm.Services.ApiClient;

public interface IAuthApiClient
{
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request);
}